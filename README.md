# Litium Smart Image Add-on
Smart Image is an Add-on for Litium platform, which analyzes and extracts rich information from images, powered by machine learning, to categorize images in a better way.
This is a base package. Use it if you want to implement an implementation of ISmartImageAnalyzer. Otherwise, you should install package like Litium.AddOns.SmartImage.GoogleCloudVision.

## Build from source
Even though the AddOn is ready to use by just installing the package, you can still build it from source.
### Requirement
1. Make sure you have set up Litium NuGet feed https://docs.litium.com/download/litium-nuget-feed
2. You need a site running equivalent version of Litium. This Add-on's version is inlined with Litium's version. If its version is 6.0.0, that means it is built against Litium 6.0.0.
3. You need to have NPM installed. [How to install NPM](https://www.npmjs.com/get-npm)

### Implement new SmartImageAnalyzer
To implement new SmartImageAnalyzer, create a class that implements ISmartImageAnalyzer interface. The 'Process' method would receive a ConcurrentQueue<ImageQueue>. We should dequeue its items and submit the request to the appropriate provider. Make sure to receive the response and update the image file to store those information.
For an example, check out the Litium.AddOns.SmartImage.GoogleCloudVision project.