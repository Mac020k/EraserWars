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

Menu: `EraserWars > Build Fish-Net Spike Scene`  
Or batch (Unity closed):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe" `
  -batchmode -nographics -projectPath (Resolve-Path .).Path `
  -executeMethod EraserWars.Editor.FishNetSpikeSceneBuilder.Build `
  -logFile TestResults\FishNet-spike-compile.log -quit
```

Scene: `Assets/Scenes/FishNet_Spike.unity` (already generated 2026-11-14)  
Play → **Start Host** (Tugboat). Optional second instance → **Start Client**.

HUD **Fire canned flick** runs `HostCommandGate` on the server, applies PhysX on the
host only, then broadcasts settle pose. On a second instance, compare
`Local vs host delta` in the HUD.

### Dual-instance smoke (operator)

1. Editor: open `FishNet_Spike`, Play, **Start Host**.
2. Build client player (Unity closed for batchmode, or menu while idle):

```powershell
# Menu: EraserWars > Build Fish-Net Spike Windows Player
# Or (Unity closed):
.\tools\run-fishnet-dual-smoke.ps1 -Build
```

3. Run the player (`Builds\FishNetSpike\EraserWars.exe`) or `.\tools\run-fishnet-dual-smoke.ps1`.
4. In the player: **Start Client** → in the editor Host: **Fire canned flick**.
5. Read `Local vs host delta` on the client HUD. Host-as-client in the same process is expected ~0.
