# charisma-sdk-unity
Unity SDK for Charisma.ai

If you have any questions or issues, please contact me at; oscar@charisma.ai

<h2> 1. Importing / Installing </h2> 

Download the contents of this repo, extract it to a folder of your choice and then import the folder to your Unity Project

<h2> 2. Settings </h2>

In the resources folder within the CharismaSDK, there is a file named Charisma Settings. This file contains all the information the plugin will use to interface with Charisma. Setting this file up correctly is important. This file needs to be referenced somewhere
in your project for the settings to be properly loaded. After that, they can be referenced anywhere from within your scripts through the "Instance" property.

  <b> 2.a StoryId </b> 
  
  The story that you want to play. The story id can be found in the URL in the following location; 
  "https://charisma.ai/stories/<StoryId>"
  
  <b> 2.b AudioConfig </b>
  
  The output format of the audio received from Charisma. the default audio format is Ogg. 
  Unless there are specific requirements that mean the format has to be Wav, I recommend keeping it as Ogg. 
  
  Audio can be received both as an audio clip (buffer) and a URL.
  
  <b> 2.c Token </b>
  
  If you want to debug your draft version (Unpublished), the StoryId needs to be set to -1 and "IsDebugging" checked. 
  You also need to retrieve your token from the Charisma website. To do this, go the Charisma website, right-click to inspect the site,
  navigate to storage and copy the access token to the "Debug token"-field.
  
<h2> 3. Example </h2>

I've created a small example for you to be able to understand how the startup flow works. To test it out, run the example scene. 
The script itself can be found in the example folder as well.
