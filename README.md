# Program Workflow

## Progress Bar

`ProgressBar` are optional UI attached to PhotonViews, and are created via `TdProgressBarUi.Spawn`.  
The `PhotonView` **NEEDS** to have the following implementations:

```
TdProgressBarUi progressBarUi {get; set;}

[PunRPC]
void StartProgressBarRpc(float duration);

[PunRPC]
void StopProgressBarRpc();
```

There is a Helper class for ITdProgressBarUi which does the job such that we don't have to implement the body ourselves.

A `ProgressBar` does something once the bar is filled up. Only the player that triggered `StartProgressBar` stores the callback. The callback only runs for local player.

## Turrets

A Turret contains `TurretExtension`, which are scripts that contains `TurretExtensionData`, which has `PhotonView`.  
`TurretExtension` is tied with `TurretExtensionData`. A new instance of `TurretExtensionData` is created on a per-turret basis.

When a Turret is created, it runs the following:  
Awake > TurretExtension > OnLoadExtension > CreatePhotonData > (RPC) OnLoad > OnLoadAfter

Each implementation of `TurretExtensionData` **NEEDS** to override OnLoadAfter, and have its `turretExtension` field assigned.  
Certain implementation of extensions require pre-initialized variables, such as `ActivateTrapExtension` requiring `ActivateTrapExtensionInit` on the Turret before Creating Photon Data.

## Extensions implemented

`FiringExtension`, which inherits from `TurretExtension`, has the following workflow after OnLoad:  
OnInteractAfter > (Set FiringExtensionData to true or false)  
UpdateTurretExtension > (if FiringExtensionData is true) > AdjustArc > Shoot > (if ReloadableExtension has ammo) > (if Animation exist) > DelayedShootProjectile  

FiringExtension currently has an issue, which is when Player1 shoots and stops using Turret, Player2 can shoot again, because the callback is null for Player2.

`ReloadableExtension`, which inherits from `TurretExtension`, has the following workflow after OnLoad:  
OnInteract > (If ammo is compatible) > StartProgressBar > LoadObject  
UpdateTurretExtension > (if ReloadableExtensionData is true) > Update ammo of Turret to match state.

`ActivateTrapExtension`, which inherits from `TurretExtension`, has the following workflow after OnLoad:  
<!-- OnInteract > (If ammo is compatible) > StartProgressBar > LoadObject   -->
UpdateTurretExtension > Update view

## Players

Players have Enum states and Emotes.