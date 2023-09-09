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

public class DrawerHandMenuSliderController : MonoBehaviour
{
    public void ApplySliderChange(DrawerHandMenuSlider slider)
    {
        //Csharp 对象传递是引用传递
        //绘画器接口
        var drawing = DrawerScript.instance;
        //当下绘画器的色彩
        var drawingColor = drawing.drawingColor;
        //控制面滑动条的长度
        var maxValueChange = slider.MRTKPinchSliderElement.parameterValues.maxValue - slider.MRTKPinchSliderElement.parameterValues.minValue;
        //把控制面板的变化传进绘画器
        switch (slider.MRTKPinchSliderElement.colorOperationEnum)
        {
            case DrawerHandMenuSlider.colorEnum.red:
                drawing.drawingColor = new Color(ApplyNewValues(slider)/ maxValueChange, drawingColor.g/ maxValueChange, drawingColor.b / maxValueChange);
                break;
            case DrawerHandMenuSlider.colorEnum.green:
                drawing.drawingColor = new Color(drawingColor.r / maxValueChange, ApplyNewValues(slider) / maxValueChange, drawingColor.b / maxValueChange);
                break;
            case DrawerHandMenuSlider.colorEnum.blue:
                drawing.drawingColor = new Color(drawingColor.r / maxValueChange, drawingColor.g / maxValueChange, ApplyNewValues(slider) / maxValueChange);
                break;
            case DrawerHandMenuSlider.colorEnum.startWidth:
                drawing.startWidth = (float)(ApplyNewValues(slider) * 0.1);
                break;
            case DrawerHandMenuSlider.colorEnum.endWidth:
                drawing.endWidth = (float)(ApplyNewValues(slider));
                break;
        }
       
        DrawerScript.instance.resultColorMesh.material.color = DrawerScript.instance.drawingColor;
    }

    public float ApplyNewValues(DrawerHandMenuSlider slider)
    {
        var parameter = slider.MRTKPinchSliderElement.parameterValues;
        //将预制滑条值转化显示值
        parameter.actualValue = parameter.minValue + (slider.MRTKPinchSliderElement.slider.SliderValue * (parameter.maxValue - parameter.minValue));
        var actualValue = parameter.actualValue;
        return actualValue;
    }
}
