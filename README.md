# Gorilla Tag Party Games
Adds multiple custom gamemodes to the VR game *Gorilla Tag*.  
Each gamemode is implemented as its own standalone mod.

## Gamemodes
### Team Infection
Two teams (Red & Blue) compete to infect all other players.  
The first team to convert everyone to their side wins.

***insert gameplay mp4 here...***

### Hot Potato
One player starts with the potato. The potato is passed by tagging another player before the countdown ends.  
Failing to pass the potato eliminates the player and allows them to slow down remaining players.

***insert gameplay mp4 here...***

## Supported Versions
| Mod Version | Compatible Gorilla Tag Version(s) |
|------------|----------------------------------|
| v1.0       | repairdigrocketdeeper.live1.1.1.129|

## Dependencies
| Library | Version Requirement |
|---------|------------------|
| Utilla  | >= v1.6.25        |

## Build
To build the project, you must specify the path to your Gorilla Tag installation.

Create a file named `Directory.Build.local.props` in the root of the repository and add the following contents:

```xml
<!-- Directory.Build.local.props -->
<Project>
    <PropertyGroup>
        <GorillaTagDir>PATH_TO_GORILLA_TAG</GorillaTagDir>
    </PropertyGroup>
</Project>
```
Replace `PATH_TO_GORILLA_TAG` with the path to your Gorilla Tag installation, for example:
```xml
<GorillaTagDir>mnt/wdblue/SteamLibrary/steamapps/common/Gorilla Tag</GorillaTagDir>
```

## Disclaimer
This product is not affiliated with Another Axiom Inc. or its videogames Gorilla Tag and Orion Drift and is not endorsed or otherwise sponsored by Another Axiom. Portions of the materials contained herein are property of Another Axiom. ©2021 Another Axiom Inc.
