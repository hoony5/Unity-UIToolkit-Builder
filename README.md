# Unity-UIToolkit-Transitions


 * Update to v0.3 - download Release unitypackage please.
     #### Change - MVC
      * Model : TransitionData.cs
      * View : UIAnimator.cs
      * Control : TransitionDataController.cs
     
     #### Change - Methods
      * Play : Play ( string visualElement's Name );
      * ReversePlay : ReversePlay ( string visualElement's Name );
      * Update : OnUpdateStyle () { Release(); Init() ; }
    
     #### Note - Now, Set Many VisualElements with Styles please. you can control each part call by name.
     
  
---

## 1. Intro

I made for easy adding or removing uss class to use unity new UI System .

## 2. SetUp

  #### 1. Create the Panel Setting.
  
  
  ![image](https://user-images.githubusercontent.com/123732566/215467563-6780aa2d-6a74-447c-8663-919d4064f999.png)
  
  
  #### 2. Place on hierarchy 'UI Animation'
  
  ![image](https://user-images.githubusercontent.com/123732566/215467799-eff2e2c9-361a-4616-b8d1-d14adae061a5.png)
 
  #### 3. Make up simple uss class.
  
  ![image](https://user-images.githubusercontent.com/123732566/215468139-36ea6c7d-b1e5-4703-9546-135bc9582370.png)

  #### 5. Create the Transition Data.
  
  ![image](https://user-images.githubusercontent.com/123732566/215468252-8258e99a-c697-4c34-a46c-a2aaad10c8e9.png)

  #### 6. Fill Data by some of the the Hieracy's Visual Elements.

  ![image](https://user-images.githubusercontent.com/123732566/215468651-52a5a742-ebb1-498e-9ca6-32bec0afb707.png)
  
  ![image](https://user-images.githubusercontent.com/123732566/215468864-202d208f-473e-4bd6-968f-e6b7fbc78aed.png)

  #### 7. Assign 'Panel Setting', Transition Data or Transition Events.
  
  ![image](https://user-images.githubusercontent.com/123732566/219416243-fb3504fd-4ce0-44b1-a9da-81da40442809.png)
  
  #### 8. Play or ReversePlay
  ![bandicam 2023-01-30 21-27-38-653(2)](https://user-images.githubusercontent.com/123732566/215484163-4906312f-b3ad-45cc-8b8d-afcd22db8977.gif)

  
## 3. Note

  #### 
