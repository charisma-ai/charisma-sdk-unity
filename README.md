# charisma-sdk-unity
Unity SDK for Charisma.ai

If you have any questions or issues, please contact me at; oscar@charisma.ai

<h2> 1. Importing / Installing </h2> 

* Download the contents of this repo, extract it to a folder of your choice and then import the folder to your Unity Project. 

<<<<<<< HEAD
8 If you get errors concerning Newtonsoft's Json Serializer, import the latest Newtonsoft.Json package from NuGet into your project.

### 2. Important Settings 

The following sections describe some the most important properties you will encounter using the SDK.

  **2.a StoryId**
  
  The Id of the story that you want to play. Each story has a unique Id. Navigate to the Charisma website, click the story you want to play from Unity. The story id can be found in the URL in the following location: 
  "https://charisma.ai/stories/>storyId<". 
  
  Pass this in the CharismaTokenSettings object.
  
  ![StoryId](https://i.ibb.co/sPqS9n2/StoryId.png)
  
  * **2.b Story Version**
  
  This is the version of the story you want to play. 
  
  * To play the latest published version, keep this at 0. 
  
  * To play a specific, published, version of your story, set the number to that particular version. 
=======
<h2> 2. Settings </h2>

In the resources folder within the CharismaSDK, there is a file named Charisma Settings. This file contains all the information the plugin will use to interface with Charisma. Setting this file up correctly is important. This file needs to be referenced somewhere
in your project for the settings to be properly loaded. After that, they can be referenced anywhere from within your scripts through the "Instance" property.

  <b> 2.a StoryId </b> 
>>>>>>> parent of a1a4179... Updated Readme
  
  The story that you want to play. The story id can be found in the URL in the following location; 
  "https://charisma.ai/stories/<StoryId>"
  
<<<<<<< HEAD
   Pass this in the CharismaTokenSettings object.
  
  **2.c Speech Options**
=======
  <b> 2.b AudioConfig </b>
>>>>>>> parent of a1a4179... Updated Readme
  
  The output format of the audio received from Charisma. the default audio format is Ogg. 
  Unless there are specific requirements that mean the format has to be Wav, I recommend keeping it as Ogg. 
  
  Audio can be received both as an audio clip (buffer) and a URL.
  
<<<<<<< HEAD
  Pass this object in the Start function whenever you start a play-through or in a Reply / Tap whenever you wish to change the type of audio you receive.
  
  **IMPORTANT:** Wav files can only be generated on Windows. Having this option selected on any other platform will result in an error.
  
  **2.d Debug Token**
  
  If you want to debug your draft version (Unpublished), the Story Version needs to be set to -1. 
  You also need to retrieve your token from the Charisma website. To do this, go the Charisma website, right-click to inspect the site,
  navigate to storage and copy the access token to the "Debug token"-field.
  
   Pass this in the CharismaTokenSettings object.
  
  **IMPORTANT:** This token renews every month or so. If you are having trouble accessing your draft version, check if the token has been renewed.
  
  ![Token](https://i.ibb.co/hfJk0H7/Token.png)
  
## 3. Example
=======
  <b> 2.c Token </b>
  
  If you want to debug your draft version (Unpublished), the StoryId needs to be set to -1 and "IsDebugging" checked. 
  You also need to retrieve your token from the Charisma website. To do this, go the Charisma website, right-click to inspect the site,
  navigate to storage and copy the access token to the "Debug token"-field.
  
<h2> 3. Example </h2>
>>>>>>> parent of a1a4179... Updated Readme

I've created a small example for you to be able to understand how the startup flow works. To test it out, create as story and run the example scene. 
The script itself can be found in the example folder as well.
<<<<<<< HEAD

## 4. Known Issues (v0.02)

* Some character voices do not convert to audio files properly, resulting in an FMOD error. The voices concerned are all named after Game of Thrones characters. Please avoid using these voices for the time being. 


=======
>>>>>>> parent of a1a4179... Updated Readme
