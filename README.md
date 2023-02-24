# Unity-UIToolkit-Transitions

## To do List

1. I'm creating Uxml Reader and Uxml to MonoScript Generator.
2. I will create USS Reader for this.
3. Fianlly, I'm designing Uxml,USS Reader - SourceGenerator (Model, Controller) - UIAnimator System. :D
* I'm looking foward to progressing work flow. ;D

## Update Note. 0.4v

1. when set transition data to dataControoler, logic ignore not contains visualElement name on the uxml.

2. test convinence : UI Animator -> ViaualElementsPanelNames : string[] work together in the buttons.

3. you can select options when start adding class or not. { transition Data }

4. AddTransitionData , bug fix- fiailed to add key .

  #### Set Test Button on the UIAnimatorEditor.cs

## Next Update 0.5v

1. Upload Uxml ,USS Reader 

2. VisualElement Name and Style Name of UIAnimator's TransitionData are going to change textField to dropdown menu. Editor Read UXml & USS.

3. Update Usage Document.

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

![startoption](https://user-images.githubusercontent.com/123732566/220820768-0a117e01-d696-40ee-a962-604afd7444a6.png)

![image](https://user-images.githubusercontent.com/123732566/215468864-202d208f-473e-4bd6-968f-e6b7fbc78aed.png)

#### 7. Assign 'Panel Setting', Transition Data or Transition Events.

![addTest](https://user-images.githubusercontent.com/123732566/220820790-f6442c2e-dfa7-49c1-ac8d-49e335aadd5e.png)

#### 8. Play or ReversePlay

![bandicam 2023-01-30 21-27-38-653(2)](https://user-images.githubusercontent.com/123732566/215484163-4906312f-b3ad-45cc-8b8d-afcd22db8977.gif)

## 3. Note

####
