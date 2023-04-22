# Unity GPTools

A collection of Unity Editor Tools that utilize ChatGPT to do interesting things.

## How to Use

Bring the **Editor** folder into your project. All the tools should show up in the top bar under *Tools -> GPT*.

The editor windows included are built with [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041), so you'll need to install that if you want to just drop these tools into an existing project.

You will need an [OpenAI API Key](https://platform.openai.com/account/api-keys) to talk with ChatGPT through Unity. Paste your key into the `API Key` field of the **OpenAI** object in the Editor/Core/OpenAI folder.

Tested with Unity 2021.3

# Tools

## Gradient Generator
![Gradient Generator Window](https://github.com/Blueteak/UnityGPTools/blob/main/ReadmeImages/GradientTool.png?raw=true)

Generates a Unity Gradient from a prompt. Both the prompt and temp(erature) affect the resulting colors from GPT, while the Soft Gradient checkbox determines how the colors are organized in the gradient object.