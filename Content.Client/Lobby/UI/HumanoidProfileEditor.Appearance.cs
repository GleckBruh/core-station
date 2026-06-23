using System.Linq;
using Content.Client._Other.ADT.Lobby.UI;
using Content.Client.Guidebook;
using Robust.Shared.IoC;
using Color = Robust.Shared.Maths.Color;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Shared.ADT.CCVar;
using Content.Shared.Guidebook;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    public event Action<List<ProtoId<GuideEntryPrototype>>>? OnOpenGuidebook;

    private ColorSelectorSliders _rgbSkinColorSelector;
    private List<SpeciesPrototype> _species = new();
    private SpeciesWindow? _speciesWindow;
    [Dependency] private readonly DocumentParsingManager _parsingMan = default!;
    private static readonly ProtoId<GuideEntryPrototype> DefaultSpeciesGuidebook = "Species";

    public void UpdateSpeciesGuidebookIcon()
    {
        SpeciesInfoButton.StyleClasses.Clear();

        var species = Profile?.Species;
        if (species is null)
            return;

        if (!_prototypeManager.Resolve<SpeciesPrototype>(species, out var speciesProto))
            return;

        // Don't display the info button if no guide entry is found
        if (!_prototypeManager.HasIndex<GuideEntryPrototype>(species))
            return;

        const string style = "SpeciesInfoDefault";
        SpeciesInfoButton.StyleIdentifier = style;
    }

    private void UpdateGenderControls()
    {
        if (Profile == null)
        {
            return;
        }

        PronounsButton.SelectId((int)Profile.Gender);
    }

    private void UpdateAgeEdit()
    {
        AgeEdit.Text = Profile?.Age.ToString() ?? "";
    }

    private void UpdateSexControls()
    {
        if (Profile == null)
            return;

        SexButton.Clear();

        var sexes = new List<Sex>();

        // add species sex options, default to just none if we are in bizzaro world and have no species
        if (_prototypeManager.Resolve(Profile.Species, out var speciesProto))
        {
            foreach (var sex in speciesProto.Sexes)
            {
                sexes.Add(sex);
            }
        }
        else
        {
            sexes.Add(Sex.Unsexed);
        }

        // add button for each sex
        foreach (var sex in sexes)
        {
            SexButton.AddItem(Loc.GetString($"humanoid-profile-editor-sex-{sex.ToString().ToLower()}-text"), (int)sex);
        }

        if (sexes.Contains(Profile.Sex))
            SexButton.SelectId((int)Profile.Sex);
        else
            SexButton.SelectId((int)sexes[0]);
    }

    private void UpdateEyePickers()
    {
        if (Profile == null)
        {
            return;
        }

        _markingsModel.SetOrganEyeColor(Profile.Appearance.EyeColor);
        EyeColorPicker.SetData(Profile.Appearance.EyeColor);
    }

    private void UpdateSkinColor()
    {
        if (Profile == null)
            return;

        var skin = _prototypeManager.Index<SpeciesPrototype>(Profile.Species).SkinColoration;
        var strategy = _prototypeManager.Index(skin).Strategy;

        switch (strategy.InputType)
        {
            case SkinColorationStrategyInput.Unary:
                {
                    if (!Skin.Visible)
                    {
                        Skin.Visible = true;
                        RgbSkinColorContainer.Visible = false;
                    }

                    Skin.Value = strategy.ToUnary(Profile.Appearance.SkinColor);

                    break;
                }
            case SkinColorationStrategyInput.Color:
                {
                    if (!RgbSkinColorContainer.Visible)
                    {
                        Skin.Visible = false;
                        RgbSkinColorContainer.Visible = true;
                    }

                    _rgbSkinColorSelector.Color = strategy.ClosestSkinColor(Profile.Appearance.SkinColor);

                    break;
                }
        }
    }

    private void UpdateSpawnPriorityControls()
    {
        if (Profile == null)
        {
            return;
        }

        SpawnPriorityButton.SelectId((int)Profile.SpawnPriority);
    }

    /// <summary>
    /// Wires the ADT species window to the new species selector button.
    /// Call this once from the editor constructor after RobustXamlLoader.Load(this)
    /// and after the old SpeciesButton.OnItemSelected handler, if that handler still exists.
    /// </summary>
    private void InitializeSpeciesWindowSelector()
    {
        NewSpeciesButton.OnToggled += args =>
        {
            if (Profile == null)
                return;

            _speciesWindow?.Dispose();

            if (!args.Pressed)
            {
                _speciesWindow = null;
                return;
            }

            _speciesWindow = new SpeciesWindow(
                Profile,
                _prototypeManager,
                _entManager,
                _controller,
                _resManager,
                _parsingMan,
                _markingManager);

            _speciesWindow.OpenCenteredLeft();

            var oldProfile = Profile.Clone();

            _speciesWindow.ChooseAction += speciesId =>
            {
                SetSpecies(speciesId);
                OnSkinColorOnValueChangedKeepColor(oldProfile);

                _speciesWindow?.Dispose();
                _speciesWindow = null;

                if (Profile != null &&
                    _prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var speciesProto))
                {
                    NewSpeciesButton.Text = Loc.GetString(speciesProto.Name);
                }

                NewSpeciesButton.Pressed = false;
            };

            _speciesWindow.OnClose += () =>
            {
                NewSpeciesButton.Pressed = false;
                _speciesWindow = null;
            };
        };
    }

    /// <summary>
    /// Refreshes the species selector.
    /// </summary>
    public void RefreshSpecies()
    {
        SpeciesButton.Clear();
        _species.Clear();

        _species.AddRange(_prototypeManager.EnumeratePrototypes<SpeciesPrototype>().Where(o => o.RoundStart));
        _species.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        var speciesIds = _species.Select(o => o.ID).ToList();

        for (var i = 0; i < _species.Count; i++)
        {
            var name = Loc.GetString(_species[i].Name);

            SpeciesButton.AddItem(name, i);

            if (Profile?.Species.Equals(_species[i].ID) == true)
            {
                SpeciesButton.SelectId(i);

                NewSpeciesButton.Text = name;
                NewSpeciesButton.Pressed = false;
                _speciesWindow?.Dispose();
                _speciesWindow = null;
            }
        }

        // If our species isn't available then reset it to default.
        if (Profile != null)
        {
            if (!speciesIds.Contains(Profile.Species))
            {
                SetSpecies(HumanoidCharacterProfile.DefaultSpecies);
            }
        }
    }

    private void SetSpecies(string newSpecies)
    {
        Profile = Profile?.WithSpecies(newSpecies);
        OnSkinColorOnValueChanged(); // Species may have special color prefs, make sure to update it.
        _markingsModel.OrganData = _markingManager.GetMarkingData(newSpecies);
        _markingsModel.ValidateMarkings();
        // In case there's job restrictions for the species
        RefreshJobs();
        // In case there's species restrictions for loadouts
        RefreshLoadouts();
        UpdateSexControls(); // update sex for new species
        UpdateSpeciesGuidebookIcon();
        ReloadPreview();
    }

    private void SetAge(int newAge)
    {
        Profile = Profile?.WithAge(newAge);
        ReloadPreview();
    }

    private void SetSex(Sex newSex)
    {
        Profile = Profile?.WithSex(newSex);
        // for convenience, default to most common gender when new sex is selected
        switch (newSex)
        {
            case Sex.Male:
                Profile = Profile?.WithGender(Gender.Male);
                break;
            case Sex.Female:
                Profile = Profile?.WithGender(Gender.Female);
                break;
            default:
                Profile = Profile?.WithGender(Gender.Epicene);
                break;
        }

        UpdateGenderControls();
        UpdateTTSVoicesControls(); // Corvax-TTS
        _markingsModel.SetOrganSexes(newSex);
        ReloadPreview();
    }




    private void SetGender(Gender newGender)
    {
        Profile = Profile?.WithGender(newGender);
        ReloadPreview();
    }

    private void SetSpawnPriority(SpawnPriorityPreference newSpawnPriority)
    {
        Profile = Profile?.WithSpawnPriorityPreference(newSpawnPriority);
        SetDirty();
    }

    private void OnSpeciesInfoButtonPressed(BaseButton.ButtonEventArgs args)
    {
        // TODO GUIDEBOOK
        // make the species guide book a field on the species prototype.
        // I.e., do what jobs/antags do.

        var guidebookController = UserInterfaceManager.GetUIController<GuidebookUIController>();
        var species = Profile?.Species ?? HumanoidCharacterProfile.DefaultSpecies;
        var page = DefaultSpeciesGuidebook;
        if (_prototypeManager.HasIndex<GuideEntryPrototype>(species))
            page = new ProtoId<GuideEntryPrototype>(species.Id); // Gross. See above todo comment.

        if (_prototypeManager.Resolve(DefaultSpeciesGuidebook, out var guideRoot))
        {
            var dict = new Dictionary<ProtoId<GuideEntryPrototype>, GuideEntry>();
            dict.Add(DefaultSpeciesGuidebook, guideRoot);
            //TODO: Don't close the guidebook if its already open, just go to the correct page
            guidebookController.OpenGuidebook(dict, includeChildren: true, selected: page);
        }
    }

    private void OnSkinColorOnValueChanged()
    {
        if (Profile is null) return;

        var skin = _prototypeManager.Index<SpeciesPrototype>(Profile.Species).SkinColoration;
        var strategy = _prototypeManager.Index(skin).Strategy;

        switch (strategy.InputType)
        {
            case SkinColorationStrategyInput.Unary:
                {
                    if (!Skin.Visible)
                    {
                        Skin.Visible = true;
                        RgbSkinColorContainer.Visible = false;
                    }

                    var color = strategy.FromUnary(Skin.Value);

                    _markingsModel.SetOrganSkinColor(color);
                    Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithSkinColor(color));

                    break;
                }
            case SkinColorationStrategyInput.Color:
                {
                    if (!RgbSkinColorContainer.Visible)
                    {
                        Skin.Visible = false;
                        RgbSkinColorContainer.Visible = true;
                    }

                    var color = strategy.ClosestSkinColor(_rgbSkinColorSelector.Color);

                    _markingsModel.SetOrganSkinColor(color);
                    Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithSkinColor(color));

                    break;
                }
        }

        ReloadProfilePreview();
    }
    // ADT Species Window start
    private void OnSkinColorOnValueChangedKeepColor(HumanoidCharacterProfile previous)
    {
        if (Profile is null)
            return;

        var skinTypeStr = _prototypeManager.Index<SpeciesPrototype>(Profile.Species).SkinColoration;
        var color = previous.Appearance.SkinColor;

        switch (skinTypeStr)
        {
            case "HumanToned":
                // Keep previous skin color.
                break;

            case "Hues":
                color = AdjustBrightness(color, 0.9f, 1.0f);
                break;

            case "TintedHues":
                color = AdjustSaturation(color, 0.1f);
                break;

            case "VoxFeathers":
                color = ClampColor(color, 29f / 360f, 174f / 360f, 0.2f, 0.88f, 0.36f, 0.55f);
                break;

            default:
                // Unknown skin color strategy: keep previous color.
                break;
        }

        _rgbSkinColorSelector.Color = color;
        ReloadProfilePreview();
    }

    private Color AdjustBrightness(Color color, float min, float max)
    {
        var hsv = Color.ToHsv(color);
        hsv.Z = Math.Clamp(hsv.Z, min, max);
        return Color.FromHsv(hsv);
    }

    private Color AdjustSaturation(Color color, float maxSaturation)
    {
        var hsv = Color.ToHsv(color);
        hsv.Y = Math.Min(hsv.Y, maxSaturation);
        return Color.FromHsv(hsv);
    }

    private Color ClampColor(Color color, float minH, float maxH, float minS, float maxS, float minV, float maxV)
    {
        var hsv = Color.ToHsv(color);
        hsv.X = Math.Clamp(hsv.X, minH, maxH);
        hsv.Y = Math.Clamp(hsv.Y, minS, maxS);
        hsv.Z = Math.Clamp(hsv.Z, minV, maxV);
        return Color.FromHsv(hsv);
    }
    // ADT Species Window end
    // ADT Barks start
    private void SetBarkProto(string prototype)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithBarkProto(prototype);
        ReloadPreview();
        SetDirty();
    }

    private void SetBarkPitch(float pitch)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithBarkPitch(Math.Clamp(
            pitch,
            _cfgManager.GetCVar(ADTCCVars.BarksMinPitch),
            _cfgManager.GetCVar(ADTCCVars.BarksMaxPitch)));

        ReloadPreview();
        SetDirty();
    }

    private void SetBarkMinVariation(float variation)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithBarkMinVariation(Math.Clamp(
            variation,
            _cfgManager.GetCVar(ADTCCVars.BarksMinDelay),
            Profile.Bark.MaxVar));

        ReloadPreview();
        SetDirty();
    }

    private void SetBarkMaxVariation(float variation)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithBarkMaxVariation(Math.Clamp(
            variation,
            Profile.Bark.MinVar,
            _cfgManager.GetCVar(ADTCCVars.BarksMaxDelay)));

        ReloadPreview();
        SetDirty();
    }
    // ADT Barks end

}
