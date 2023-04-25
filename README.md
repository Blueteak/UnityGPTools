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

## Chat Box
![Chat Box Window](https://github.com/Blueteak/UnityGPTools/blob/main/ReadmeImages/ChatWindow.png?raw=true)

A simple interface for using ChatGPT like you would on the website. This uses data streaming to generate the text token-by-token in realtime, just like the site does! Each message sends the **entire** message chain to the API (this is how followup questions retain context), which means each followup question sent is more expensive.

It also allows selecting text from the AI responses in case you want to copy/paste from it.

## Regex Patter Generator
![Regular Expression Generator Window](https://github.com/Blueteak/UnityGPTools/blob/main/ReadmeImages/RegexBuilder.png?raw=true)

This one is a bit hit-or-miss, but it's really great when it works! Just input a full text sample and the desired regex-patter-match output and hit generate. 

It should create a preview of the regex pattern text, and you can quickly test it on the example text box by hitting the 'Test Regex' button.