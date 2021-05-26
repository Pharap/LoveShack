# LoveShack

One-stop shop for packaging LÖVE files for Windows.

## What Does It Do?

It acts as a very simple way to generate:
* A `.love` file.
* An `.exe` file.
* A `.zip` containing the `.exe` file and accompanying `.dll` files.

And when I say 'very simple' I mean so horrendously simple that if you're expecting it to do anything fancier or smarter than that, like check that it's actually done its job properly, then I'm afraid you'll be out of luck.

## Why?

Most packaging tools for LÖVE are complicated, have lots of fancy features, or depend on some other tool or library.
I wanted something quick, simple, easy to understand, easy to edit, cheap to install and redistribute, and requiring as few extra components (for a C# developer) as possible.  

## How To Use

### First Use Steps

1. Compile the program.
2. Manually edit the `LoveShack.exe.config` file to point it to your `love.exe` file.
  * See [Setting The LÖVE Path](#Setting_The_LÖVE_Path) section below.

### Further Steps

1. Drag the folder containing your `main.lua`, `conf.lua` and all other relevant files onto `LoveShack.exe`.
  * Note: this would be the folder that you'd ordinarily drag onto `love.exe` itself to run the code.
2. Be patient.
3. Enjoy your spoils of war.

## Setting The LÖVE Path

At the moment the cannonical way to set the `love.exe` path is to manually edit the `LoveShack.exe.config` file with a text editor.

By default it will look something like this:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
            <section name="LoveShack.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" allowExeDefinition="MachineToLocalUser" requirePermission="false" />
        </sectionGroup>
    </configSections>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1" />
    </startup>
    <userSettings>
        <LoveShack.Properties.Settings>
            <setting name="LovePath" serializeAs="String">
                <value />
            </setting>
        </LoveShack.Properties.Settings>
    </userSettings>
</configuration>
```
But you can ignore 90% of that and just focus on the `<value />` line.
That's the part you replace with `<value>~path to your love.exe file~</love>`, e.g. `<value>C:\Program Files\LOVE\love.exe</love>`.

After you've done that, you need never touch the file again providing your `love.exe` never moves.

## Future Features

At some point I'll probably make it auto-detect the file if it's dropped in the same directory and the settings file isn't already set.

That aside, I don't think I'm likely to add any other features to this.

Pretty much anything I could hope to add would likely mean parsing command line arguments and that would be more effort than I can be bothered to spend on this project.

At the most I might separate this into three separate executables that do different jobs and provide a script that calls them in order or something.
