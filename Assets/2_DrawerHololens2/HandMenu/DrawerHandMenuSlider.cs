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
        //���ýṹ��
        public PinchSlider slider;
        //�Խ��ṹ��
        public ParameterValues parameterValues; 
    }

    [Serializable]
    public class ParameterValues
    {
        public float minValue;
        public float maxValue;
        public float actualValue;
    }

    //�Խ��ṹ��
    public MRTKPinchSlider MRTKPinchSliderElement;

    public void Start()
    {

        var values = MRTKPinchSliderElement.parameterValues;
        //����һ���滭���Ľӿ�
        var drawingColor = DrawerScript.instance;
        //�ѿ������Ŀ��Ʋ��������滭��
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

        //���ÿ���������ʾ
        MRTKPinchSliderElement.slider.SliderValue =  (values.actualValue - values.minValue) / (values.maxValue - values.minValue);
    }
}
