# Program Workflow

## Progress Bar

Players have `ProgressBar`, which does something once the bar is filled up. When the bar is ran through RPC, the local player stores the callback tied to the bar. The callback only runs for local player.

## Turrets

A Turret contains `TurretExtension`, which are scripts that contains `TurretExtensionData`, which has `PhotonView`.

When a Turret is created, it runs the following:  
Turret > TurretExtension > OnLoadExtension > CreatePhotonData > (RPC) OnLoad

`FiringExtension`, which inherits from `TurretExtension`, has the following workflow after OnLoad:  
OnInteractAfter > (Set FiringExtensionData to true or false)  
UpdateTurretExtension > (if FiringExtensionData is true) > AdjustArc > Shoot > (if ReloadableExtension has ammo) > (if Animation exist) > DelayedShootProjectile  

`ReloadableExtension`, which inherits from `TurretExtension`, has the following workflow after OnLoad:  
OnInteract > (If ammo is compatible) > StartProgressBar > LoadObject  
UpdateTurretExtension > (if ReloadableExtensionData is true) > Update ammo of Turret to match state.

## Players

Players have Enum states and Emotes.