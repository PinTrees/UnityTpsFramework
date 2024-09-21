using UnityEngine;

public class ColorEx
{
    public static Color Transparent = new Color(0, 0, 0, 0);
    public static Color White = Color.white;

    public static Color FromHex(string hex)
    {
        // 16진수 색상 코드 앞에 '#'가 있을 경우 제거
        hex = hex.Replace("#", "");
        hex.ToLower();

        // 16진수 길이에 따라 색상 값을 추출
        if (hex.Length == 6) // RRGGBB 형태
        {
            float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            return new Color(r, g, b);
        }
        else if (hex.Length == 8) // RRGGBBAA 형태
        {
            float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float a = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            return new Color(r, g, b, a);
        }
        else
        {
            throw new System.ArgumentException("Invalid hex string length. It should be either 6 or 8 characters long.");
        }
    }
}
