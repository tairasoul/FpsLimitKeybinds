# FpsLimitKeybinds

simple mod to set keybinds for fps limits. made for va proxy initially but should work regardless of game

config file used is `BepInEx/config/keybinds.toml`, keybinds are in the `keybinds` array, formatted as `"keycombo:fps"`

for example, `"ctrl+shift+g:60"`

if you put `unlimited` where the fps is, it will uncap your fps.

non-modifiers are parsed directly into a [KeyCode](https://docs.unity3d.com/ScriptReference/KeyCode.html) object, if you need something that isn't a regular key (say, a controller button), go on here and instead of doing, say, `ctrl+shift+a:60`, you do `joystickbutton0:60` (unsure which joystickbutton_ is which controller key)

example config:

```toml
keybinds = ["ctrl+shift+h:20", "ctrl+shift+j:60", "ctrl+shift+g:unlimited"]
```