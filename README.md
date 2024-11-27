# FpsLimitKeybinds

simple mod to set keybinds for fps limits. made for va proxy initially but should work regardless of game

config file used is `BepInEx/config/keybinds.toml`, keybinds are in the `keybinds` array, formatted as `"keycombo:fps"`

for example, `"ctrl+shift+g:60"`

if you put `unlimited` where the fps is, it will uncap your fps.

example config:

```toml
keybinds = ["ctrl+shift+h:20", "ctrl+shift+j:60", "ctrl+shift+g:unlimited"]
```