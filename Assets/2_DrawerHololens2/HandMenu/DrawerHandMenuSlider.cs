/* 
*  __  __   ______   _____    _______ 
* |  \/  | |  ____| |  __ \  |__   __|
* | \  / | | |__    | |__) |    | |   
* | |\/| | |  __|   |  _  /     | |   
* | |  | | | |____  | | \ \     | |   
* |_|  |_| |______| |_|  \_\    |_|   
*                                     
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using System;

public class DrawerHandMenuSlider : MonoBehaviour
{
    public enum colorEnum
    {
        red,green,blue,startWidth,endWidth
    }

    [Serializable]
    public class MRTKPinchSlider
    {
        public colorEnum colorOperationEnum;
        //引用结构体
        public PinchSlider slider;
        //自建结构体
        public ParameterValues parameterValues; 
    }

    [Serializable]
    public class ParameterValues
    {
        public float minValue;
        public float maxValue;
        public float actualValue;
    }

    //自建结构体
    public MRTKPinchSlider MRTKPinchSliderElement;

    public void Start()
    {

        var values = MRTKPinchSliderElement.parameterValues;
        //创建一个绘画器的接口
        var drawingColor = DrawerScript.instance;
        //把控制面板的控制参数传给绘画器
        switch (MRTKPinchSliderElement.colorOperationEnum)
        {
            case colorEnum.red:
                values.actualValue = drawingColor.drawingColor.r;
                break;
            case colorEnum.green:
                values.actualValue = drawingColor.drawingColor.g;
                break;
            case colorEnum.blue:
                values.actualValue = drawingColor.drawingColor.b;
                break;
            case colorEnum.startWidth:
                values.actualValue = drawingColor.startWidth;
                break;
            case colorEnum.endWidth:
                values.actualValue = drawingColor.endWidth;
                break;
        }

        //设置控制面板的显示
        MRTKPinchSliderElement.slider.SliderValue =  (values.actualValue - values.minValue) / (values.maxValue - values.minValue);
    }
}
