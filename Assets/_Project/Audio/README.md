# Audio

`AudioBus` plays a looping bed and four short cues. Volume comes from `PlayerPrefs`.

## Prefs contract

`EraserWars.Audio.GameSettings` owns the keys. Store each slider's value as percent. Do not store a 0 to 1 gain. `AudioBus` turns percent into linear gain with `percent / 100`. 0 is silent. 100 is full scale.

| Constant | PlayerPrefs name | Default | Slider name |
| --- | --- | --- | --- |
| `BgmPercentKey` | `eraserwars.settings.bgmPercent` | 70 | `opt-bgm` |
| `SePercentKey` | `eraserwars.settings.sePercent` | 80 | `opt-se` |

Call `SaveBgmPercent` and `SaveSePercent`, or `PlayerPrefs.SetFloat` with those names. `AudioBus` reads the keys every frame, so a write shows up on the next frame without another call.

The sliders already exist in `Assets/UI/StartMenu.uxml`. `AudioBus` finds `opt-bgm` and `opt-se` and saves their new values. It does not change that file. On load it keeps a saved percent. It does not copy the slider's starting value over a saved percent. When the options screen opens, set each slider from the saved percent.

## What plays

| Moment | Clip | Bus |
| --- | --- | --- |
| `AudioBus` starts | `bgm-loop`, looping | BGM |
| A `Button` click under a loaded `UIDocument` | `ui-click` | SE |
| `MatchDirector.SubmitFlick` accepts a flick | `flick` | SE |
| `EraserCollisionRelay` accepts an eraser pair | `eraser-hit` | SE |
| `FalloutDetector` hits a Floor collider, once until `ResetReport` | `floor-hit` | SE |

The clips are generated in code. They are not files under `Assets/Audio`. They stand in until recorded eraser hits and purchased music replace them. `AudioBus` uses `DontDestroyOnLoad`, so the loop keeps playing when the start menu hides.
