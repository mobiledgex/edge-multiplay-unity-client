# Changelog
All notable changes to this EdgeMultiplay Unity Client will be documented in this file.

## [1.2.0] - 2021-05-05

### Fix & Improvements:
- Fixed App Quiting error on PC/Mac Unity Player.
- Fixed customizable fallback location for Oculus/Non Phone devices in your GameManager.cs
```
EdgeManager.integration.useFallbackLocation = true;
EdgeManager.integration.setFallbackLocation(longitude, latitude);
ConnectToEdge(useFallBackLocation:true);
```
- Use SendGamePlayEvent() and SendGamePlayEventUDP() in your PlayerManager to SendGamePlayEvents.

### New Features:
- Server Stats, Now you can see the live server stats from EdgeMultiplay Menu in Unity Editor.
- New Ownership Transfer options : TakeOver and RequestOwnership.
- UpdateRate (Fixed Update or EveryFrame) for EdgeMultiplayObserver.
- New Example Scenes added (Chat Rooms, OwnershipExamples)
- New notification callback added OnRoomRemovedFromLobby()


## [1.1.0] - 2021-04-01

### Fix & Improvements.
- Custom Tags (App Dependent Tags) added to Players.
- Observables ownership transfer, Creating observables at runtime.
- Added Compatibility with Oculus SDK.
- OrphanObservable added, Add OrphanObservable Component to GameObjects that don't have an owner at the time of creation (Ex. created in Unity Editor).
- Example Scenes added (Creating synced objects at runtime, ownership transfer)


## [1.0.0] - 2021-02-12

### Fix & Improvements.
- First release for EdgeMultiplay Unity Client
- Getting started tutorials added https://www.youtube.com/watch?v=9kMz6Q3g0xQ&list=PLwUZZfaECSv18E5d0ooDR7S8416pImW8W
- Example Scenes added (ChatExample, PingPongExample, ARPingPongExample)
