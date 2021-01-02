# Among Us Game Reader
This client reads the game data of an among us game and relays it to [Swissbot](https://github.com/quinchs/SwissbotCore) so it can control VC.

I wrote this client for my vc dashboard, allowing mods to enable auto mute for among us games with public members, it will also allow for auto deafen games so ghosts can talk.

# How? Theres no among us SDK

Well its simple really... Tears! This client reads the memory of among us and finds values based off of offsets. so for example the lobby code lives in the `GameAssembly.dll`,
so we have to find that dll's memory address in the exe (`0x0000000006ef0000`) and then we have to get an address of the lobby code 
based off of some offsets (`20607324, 92, 0, 32, 40`). A few jumps around the stack later and we arrive at a pointer to the game code (`0x0000000009e54fa0`), 
reading it as a string gives us the current game code! See just a few tears and your there! 

# How do you know the offsets?

Well, some people with more patience than me dug thru the games memory and painfully mapped out all the important variables and how there stored, I simply took there research and implemented it.


# Couldn't this be used for hacking/cheating?

Yes it could, most cheats are based off of memory mapping and code injection, but its your responsibility to ethically use this method of getting data, not mine!

# How can I use your code for my own project?

If enough people ask, I'll release a version thats documented and has much better functionality and I will put it on nuget.
