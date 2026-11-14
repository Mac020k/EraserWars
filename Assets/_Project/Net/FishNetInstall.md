# Fish-Net install (P1-5 / P6)

## Current install (2026-11-14)

Fish-Net **4.7.4** lives under `Assets/FishNet` (not UPM).

Why not UPM git: the free package ships a stripped **Yak** transport that fails
CS0103 / CS1061 on Unity `6000.6.9f1`. We deleted `Runtime/Plugins/Yak` and use
**Tugboat** only.

Demos were removed to keep the repo smaller.

## Refresh / reinstall

```powershell
# From repo root, with Unity closed:
Remove-Item -Recurse -Force Assets\FishNet -ErrorAction SilentlyContinue
git clone --depth 1 --branch 4.7.4 https://github.com/FirstGearGames/FishNet.git Temp\FishNetClone
Copy-Item -Recurse Temp\FishNetClone\Assets\FishNet Assets\FishNet
Remove-Item -Recurse -Force Assets\FishNet\Runtime\Plugins\Yak, Assets\FishNet\Runtime\Plugins\Yak.meta, Assets\FishNet\Demos, Assets\FishNet\Demos.meta -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force Temp\FishNetClone
```

Then open Unity once so `.meta` files regenerate.

## Spike scene

Menu: `Keshipin > Build Fish-Net Spike Scene`  
Or batch (Unity closed):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe" `
  -batchmode -nographics -projectPath (Resolve-Path .).Path `
  -executeMethod Keshipin.Editor.FishNetSpikeSceneBuilder.Build `
  -logFile TestResults\FishNet-spike-compile.log -quit
```

Scene: `Assets/Scenes/FishNet_Spike.unity` (already generated 2026-11-14)  
Play → **Start Host** (Tugboat). Optional second instance → **Start Client**.

`HostCommandGate` is ready for the next step: send one `FlickCommand` on the host
and compare settle poses.
