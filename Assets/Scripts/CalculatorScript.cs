using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using System.Text.RegularExpressions;

public class CalculatorScript : MonoBehaviour
{
    [SerializeField] Text input;
    [SerializeField] Text equation;
    int bracketCount = 0;
    bool isEvaluated = false;
    string[] operators = { "+", "-", "x", "%", "/" };

    public void OnNumPressed(int num)
    {
        if (equation.text.EndsWith(")"))
        {
            return;
        }
        if (isEvaluated)
        {
            input.text = "";
        }
        input.text += num.ToString();
        FormatInput();
        isEvaluated = false;
    }

    public void OnDotPressed()
    {
        if (isEvaluated)
        {
            input.text = "0.";
            return;
        }

        //check if a dot already exits in inputStr
        if (input.text.Contains("."))
        {
            return;
        }
        input.text += ".";
    }

    public void OnOpPressed(string operation)
    {
        if(equation.text.Length == 0 && input.text.Length == 0)
        {
            return;
        }

        if (EndsWith(equation.text.Trim(), operators) && input.text.Length == 0)
        {
            if (operation == "=")
            {
                return;
            }
            else
            {
                equation.text = equation.text.Remove(equation.text.Length - 2) + operation + " ";
            }
            return;
        }


        if (operation == "=")
        {
            if(bracketCount != 0)
        {
            return;
        }
            equation.text += input.text;
            input.text = Evaluate(equation.text);
            FormatInput();
            equation.text = "";
            isEvaluated = true;
            return;
        }

        if (isEvaluated)
        {
            equation.text = "";
            equation.text += input.text + " " + operation + " ";
            ResetInput();
            isEvaluated = false;
            return;
        }


        equation.text += input.text + " " + operation + " ";
        ResetInput();
        isEvaluated = false;
    }

    public void OnNegPressed()
    {
        if(input.text.Length == 0 || input.text == "0")
        {
            return;
        }

        if (input.text.Contains("-"))
        {
            input.text = input.text.Remove(0, 1);
        }
        else
        {
            input.text = input.text.Insert(0, "-");
        }
        isEvaluated = false;
    }

    public void OnBracketPressed(string bracket)
    {
        if (isEvaluated)
        {
            ResetInput();
            ResetEquation();
        }


        if(bracket == "(")
        {
            if (input.text.Length != 0)
            {
                return;
            }

            if(EndsWith(equation.text.Trim(), operators) || equation.text.Length == 0 || equation.text.EndsWith("("))
            {
                equation.text += bracket;
                bracketCount++;
                isEvaluated = false;
            }
        }
        else
        {
            if(bracketCount <= 0)
            {
                return;
            }

            if((EndsWith(equation.text.Trim(), operators) || equation.text.EndsWith("(")) && input.text.Length == 0)
            {
                return;
            }

            bracketCount--;
            equation.text += input.text + bracket;
            input.text = "";
        }
    }

    public void OnDelPressed()
    {
        if(input.text.Length == 0 || isEvaluated )
        {
            input.text = "";
            return;
        }
        input.text = input.text.Remove(input.text.Length - 1);
        FormatInput();
        if (input.text == "-")
        {
            input.text = "";
        }
    }

    public void OnClearPressed()
    {
        ResetInput();
        ResetEquation();
        bracketCount = 0;
        isEvaluated = false;
    }

    private void FormatInput()
    {
        if (Regex.Match(input.text, @"\b*.0+\b").Success)
        {
            return;
        }
        decimal inputDec;
        if(!decimal.TryParse(input.text, out inputDec))
        {
            inputDec = 0;
        }
        else
        {
            input.text = string.Format("{0:0,0.################}", inputDec);
            if (input.text.StartsWith("0")) {
                input.text = input.text.Remove(0, 1);      
            }
            if (!input.text.StartsWith("0."))
            {
                input.text = input.text.TrimStart(new char[] { '0' });
            }
            if (input.text.Length == 0)
            {
                input.text = "0";
            }
        }
    }

    private string Evaluate(string expression)
    {
        DataTable dt = new DataTable();

        string eq = RefineEquationString();
        try
        {
            var v = dt.Compute(eq, "");
            string result = v.ToString();
            if (result.Contains("."))
            {
                result = result.TrimEnd(new char[] { '0' });
            }
            if (result.Length == 0)
            {
                result += "0";
            }
            return result;
        }
        catch (System.Exception)
        {
            return "";
        }
    }

    private string RefineEquationString()
    {
        string eq = equation.text.Replace("x", "*");

        eq = eq.Replace("%", "* 0.01 *");

        eq = eq.Replace(",", "");

        return eq;
    }

    private void ResetInput()
    {
        input.text = "";
    }

    private void ResetEquation()
    {
        equation.text = "";
    }

    private bool EndsWith(string value, string[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (value.EndsWith(options[i]))
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        ResetInput();
        ResetEquation();
        LoadState();
    }
    void OnApplicationQuit()
    {
        SaveState();
    }

    private void SaveState()
    {
        PlayerPrefs.SetString("input", input.text);
        PlayerPrefs.SetString("equation", equation.text);
        PlayerPrefs.SetInt("bracketCount", bracketCount);
        PlayerPrefs.SetInt("isEvaluated", isEvaluated ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        input.text = PlayerPrefs.GetString("input");
        equation.text = PlayerPrefs.GetString("equation");
        bracketCount = PlayerPrefs.GetInt("bracketCount");
        isEvaluated = PlayerPrefs.GetInt("isEvaluated") == 1;
    }
}
