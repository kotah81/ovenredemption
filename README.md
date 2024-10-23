
![ovenredemptiontitle](https://github.com/user-attachments/assets/7d65c04e-e6ce-4394-b4cd-4e32f8be87d0)
> ovenredemption is a program for Pizza Tower mods with a sharp focus on retro-compatibilty and extreme simplicity.

### Notes:
- The name is a callback to PizzaOven, however it isn't a successor in any way or shape...
- Just like PizzaOven, ovenredemption doesn't let you load multiple mods at the same time.
- ovenredemption was made via WPF. (WPF: Windows Presentation Foundation)

# HOW IT WORKS
- I. ovenredemption registers **specically structured zip files** (called modpacks or redemption zips).
> _Modpacks are made to contain a steam manifest, xdelta files and all necessary extra mod files._
![zipstructureexample](https://github.com/user-attachments/assets/9ce00846-5c42-49d5-aea2-24d2cda4dc39)
- II. The program then prompts the user to enter their Steam credentials.
> Those are necessary in order to legally download any Steam release of Pizza Tower.
- III. After acquiring steam credentials, ovenredemption launches a [DepotDownloader](https://github.com/SteamRE/DepotDownloader/forks) process.
> Which downloads the PT release specified in the modpack at the _%appdata%/Roaming/OvenRedemption/Game_ directory.
- IV. Once the download is done, ovenredemption starts applying the present xdelta patches.
> If there's also extra files, the program will also paste those in.
- V. If all previous steps happened successfully, the game will launch!
> _+ The program deletes steam_api64.dll to prevent Steam from launching its own install._

#### Note: ovenredemption does also recognize previous mod installs and lets you launch them instantly.

# Q&A
Can you make a modpack that only contains a manifest to play older versions of vanilla Pizza Tower?
> Yes.

Does ovenredemption steal my Steam logins?
> Yes, it does.

Can I undo the stealing?
> No, you can't.

Is the author of this project Sertif?
> No, unfortunately.

## Releasing a beta within the next few days!
![appgif](https://github.com/user-attachments/assets/6eadacb7-c68f-41f8-a9b6-0584877e7886)
