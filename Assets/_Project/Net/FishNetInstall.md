# Fish-Net install (P1-5 / P6)

Unity Editor must be closed before package install. Batchmode and the open
editor share `Temp/UnityLockfile`.

## Goal of the spike

Prove Fish-Net 4.x loads on Unity `6000.6.9f1`, hosts a Tugboat LAN session,
and can carry one `FlickCommand` through `HostCommandGate` so both peers see
the same eraser settle hash for one shot.

## Steps

1. Quit every Unity process that has this project open. Confirm
   `Temp/UnityLockfile` is gone.
2. Install Fish-Net 4.x via one of:
   - OpenUPM / git package URL into `Packages/manifest.json`
   - Asset Store import into `Assets/Plugins/FishNet` (prefer Packages)
3. Open the project. Fix any CS0619 / API break immediately and note it in
   [ADR-0008](../../../docs/decisions/ADR-0008-fishnet-spike.md).
4. Create a throwaway scene `Assets/Scenes/FishNet_Spike.unity` with:
   - NetworkManager + Tugboat
   - Host button and Client button (debug OnGUI is fine)
   - A single networked eraser cube owned by the host for physics
5. On the host, call `HostCommandGate` with a canned `FlickCommand`, apply
   via existing `FlickApplier` on the host only (ADR-0002 host physics).
6. Record whether client sees the same end pose within tolerance.

## Out of scope for the spike

- Steam transport / FishySteamworks
- Ranked lockstep
- Card sync

## Failure path

If Fish-Net fails to compile on 6000.6.9f1 within one day, stop and update
ADR-0008 to evaluate NGO 2.14 + community Steam transport.

`HostCommandGate` already validates phase and player before reducer apply.
