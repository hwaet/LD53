using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UIElements;
using static Blazon;
using static Charge;
using static Field;
using static Ordinary;

public static class BlazonDrawers  {

    #region Main Drawing Functions

    public static Texture2D ApplyDivision(Texture2D field, Division div, Color[] tinctures, bool sinister = false) {
        switch (div) {
            case Division.Fess:
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(0, 0, field.width, field.height / 2), tinctures[1]);
                return field;
            case Division.Pale:
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(field.width / 2, 0, field.width / 2, field.height), tinctures[1]);
                return field;
            case Division.Bend:
                if (!sinister) {
                    field = Fill(field, tinctures[0]);
                    field = DrawLineSegment(field, new Vector2Int(0, field.height - 1), new Vector2Int(field.width - 1, 0), tinctures[1]);
                    field = FloodFill(field, new Vector2Int(0, 0), tinctures[1], FloodFillMode.AllSeedColor);
                }
                else {
                    field = Fill(field, tinctures[0]);
                    field = DrawLineSegment(field, new Vector2Int(field.width - 1, field.height - 1), new Vector2Int(0, 0), tinctures[1]);
                    field = FloodFill(field, new Vector2Int(field.width - 1, 0), tinctures[1], FloodFillMode.AllSeedColor);
                }
                return field;
            case Division.Chevron:
                field = Fill(field, tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(0, 0), new Vector2Int(field.width / 2, field.height / 3), tinctures[1]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height / 3), new Vector2Int(field.width, 0), tinctures[1]);
                field = FloodFill(field, new Vector2Int(1, 0), tinctures[1], FloodFillMode.UntilTargetColor);
                return field;
            case Division.Cross:
                field = Fill(field, tinctures[0]);
                field = DrawRect(field, new RectInt(field.width / 2, field.height / 2, field.width / 2, field.height / 2), tinctures[1]);
                field = DrawRect(field, new RectInt(0, 0, field.width / 2, field.height / 2), tinctures[1]);
                return field;
            case Division.Saltire:
                field = Fill(field, tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(0, 0), new Vector2Int(field.width - 1, field.height - 1), tinctures[1]);
                field = DrawLineSegment(field, new Vector2Int(field.width - 1, 0), new Vector2Int(0, field.height - 1), tinctures[1]);
                field = FloodFill(field, new Vector2Int(0, field.height / 2), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, field.height / 2), tinctures[1], FloodFillMode.UntilTargetColor);
                return field;
            case Division.Pall:
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(0, field.height - 1), tinctures[0]);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(field.width - 1, field.height - 1), tinctures[0]);
                field = FloodFill(field, new Vector2Int(field.width / 2, field.height - 1), tinctures[0], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, 0), tinctures[1], FloodFillMode.AllSeedColor);
                field = DrawLineSegment(field, new Vector2Int(field.width / 2, field.height * 2 / 3), new Vector2Int(field.width / 2, 0), tinctures[2]);
                field = FloodFill(field, new Vector2Int(field.width - 1, 0), tinctures[2], FloodFillMode.AllSeedColor);


                //field = FloodFill(field, new Vector2Int(field.width - 1, 0), tinctures[2], FloodFillMode.AllSeedColor);
                return field;
            default:
                break;
        }
        return field;

    }

    public static Texture2D ApplyVariation(Texture2D field, Variation variation, Color[] tinctures, int patternCount = 4, bool sinister = false) {
        int patternStep = field.width / patternCount;
        int colorDex;

        switch (variation) {
            case Variation.Paly:
                field = Fill(field, tinctures[0]);
                colorDex = 0;
                for (int x = 0; x < field.width; x++) {
                    if (x % patternStep == 0) {
                        colorDex = (colorDex + 1) % tinctures.Length;
                    }
                    for (int y = 0; y < field.height; y++) {
                        field.SetPixel(x, y, tinctures[colorDex]);
                    }
                }
                return field;
            case Variation.Barry:
                field = Fill(field, tinctures[0]);
                colorDex = 0;
                for (int y = field.height - 1; y >= 0; y--) {
                    if (y % patternStep == 0) {
                        colorDex = (colorDex + 1) % tinctures.Length;
                    }
                    for (int x = 0; x < field.width; x++) {
                        field.SetPixel(x, y, tinctures[colorDex]);
                    }
                }
                return field;
            case Variation.Bendy:
                colorDex = 0;
                int offset = 0;
                if (sinister) {

                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            if ((x + offset) % patternStep == 0) {
                                colorDex = (colorDex + 1) % tinctures.Length;
                            }
                            field.SetPixel(x, y, tinctures[colorDex]);
                        }
                        offset++;
                        if ((y + 1) % patternStep == 0) {
                            colorDex--;
                            if (colorDex < 0) {
                                colorDex = tinctures.Length - 1;
                            }
                        }
                    }
                }
                else {
                    colorDex = 0;
                    offset = field.width;
                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            if ((x + offset) % patternStep == 0) {
                                colorDex = (colorDex + 1) % tinctures.Length;
                            }
                            field.SetPixel(x, y, tinctures[colorDex]);
                        }
                        offset--;
                        if (y % patternStep == 0) {
                            colorDex--;
                            if (colorDex < 0) {
                                colorDex = tinctures.Length - 1;
                            }
                        }
                    }
                }
                return field;
            case Variation.Chequy:
                for (int x = 0; x < field.width; x++) {
                    for (int y = 0; y < field.height; y++) {
                        if ((x / patternStep + y / patternStep) % 2 == 0) {
                            field.SetPixel(x, y, tinctures[1]);
                        }
                        else {
                            field.SetPixel(x, y, tinctures[0]);
                        }
                    }
                }
                return field;
            case Variation.Chevronny:
                field = Fill(field, tinctures[0]);
                int colorDexLeft = 0;
                int colorDexRight = 1;
                int offsetLeft = 0;
                int offsetRight = field.width;

                for (int y = field.height - 1; y >= 0; y--) {
                    for (int x = 0; x < field.width / 2; x++) {
                        if ((x + offsetLeft) % patternStep == 0) {
                            colorDexLeft = (colorDexLeft + 1) % tinctures.Length;
                        }
                        field.SetPixel(x, y, tinctures[colorDexLeft]);
                    }

                    for (int x = field.width / 2; x < field.width; x++) {
                        if ((x + offsetRight) % patternStep == 0) {
                            colorDexRight = (colorDexRight + 1) % tinctures.Length;
                        }
                        field.SetPixel(x, y, tinctures[colorDexRight]);
                    }
                    if ((y + 1) % patternStep == 0) {
                        colorDexLeft--;
                        if (colorDexLeft < 0) {
                            colorDexLeft = tinctures.Length - 1;
                        }
                    }
                    if (y % patternStep == 0) {
                        colorDexRight--;
                        if (colorDexRight < 0) {
                            colorDexRight = tinctures.Length - 1;
                        }
                    }
                    offsetLeft++;
                    offsetRight--;

                }
                return field;
            case Variation.Gyronny:
                field = Fill(field, tinctures[0]);
                Vector2Int center = new Vector2Int(field.width / 2, field.height / 2);
                field = DrawLineSegment(field, center, new Vector2Int(0, field.height / 2), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(0, field.height - 1), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width / 2, field.height - 1), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, field.height - 1), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, field.height / 2), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(field.width - 1, 0), tinctures[1]);

                field = DrawLineSegment(field, center, new Vector2Int(field.width / 2, 0), tinctures[1]);
                field = DrawLineSegment(field, center, new Vector2Int(0, 0), tinctures[1]);

                field = FloodFill(field, new Vector2Int(0, field.height * 3 / 4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width * 3 / 4, field.height - 1), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, field.height / 4), tinctures[1], FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width / 4, 0), tinctures[1], FloodFillMode.UntilTargetColor);

                return field;

            case Variation.Lozengy:
                field = Fill(field, tinctures[0]);
                for (int x = 0; x < patternCount; x++) {
                    for (int y = 0; y < patternCount+1; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep, y * patternStep, patternStep, patternStep), tinctures[1]);

                    }
                }
                return field;
            case Variation.Fusilly:
                field = Fill(field, tinctures[0]);
                for (int x = 0; x < patternCount * 2; x++) {
                    for (int y = 0; y < patternCount +1; y++) {
                        field = DrawLozenge(field, new RectInt(x * patternStep / 2, y * patternStep, patternStep / 2, patternStep), tinctures[1]);

                    }
                }
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", variation);
                return field;


        }
    }

    public static Texture2D ApplyOrdinary(Texture2D field, OrdinaryShape shape, Color tincture, bool sinister = false) {
        switch (shape) {
            case OrdinaryShape.Chief:
                field = DrawRect(field, new RectInt(0, field.height * 2 / 3, field.width - 1, field.height - 1), tincture);
                return field;
            case OrdinaryShape.Fess:
                field = DrawRect(field, new RectInt(0, field.height / 2 - field.height / 8, field.width - 1, field.height / 4), tincture);
                return field;
            case OrdinaryShape.Pale:
                field = DrawRect(field, new RectInt(field.width / 2 - field.width / 8, 0, field.width / 4, field.height), tincture);
                return field;
            case OrdinaryShape.Cross:
                field = DrawRect(field, new RectInt(0, field.height / 2 + field.height / 10, field.width, field.height / 5), tincture);
                field = DrawRect(field, new RectInt(field.width / 2 - field.width / 10, 0, field.width / 5, field.height), tincture);
                return field;
            case OrdinaryShape.Bend:
                if (sinister) {
                    field = DrawLine(field, new Vector2Int(0, 0), new Vector2Int(1, 1), tincture, field.width / 8);
                }
                else {
                    field = DrawLine(field, new Vector2Int(0, field.height), new Vector2Int(1, -1), tincture, field.width / 8);
                }
                return field;
            case OrdinaryShape.Saltire:
                field = DrawLineSegment(field, new Vector2Int(field.width / 8, 0), new Vector2Int(field.width - 1, field.height * 7 / 8), tincture);
                field = DrawLineSegment(field, new Vector2Int(0, field.height / 8), new Vector2Int(field.width * 7 / 8, field.height - 1), tincture);

                field = DrawLineSegment(field, new Vector2Int(field.width / 8, field.height - 1), new Vector2Int(field.width - 1, field.height / 8), tincture);
                field = DrawLineSegment(field, new Vector2Int(0, field.width * 7 / 8), new Vector2Int(field.width * 7 / 8, 0), tincture);

                field = FloodFill(field, new Vector2Int(field.width / 2, field.height / 2), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, 0), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(0, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                field = FloodFill(field, new Vector2Int(field.width - 1, 0), tincture, FloodFillMode.UntilTargetColor);
                return field;

            case OrdinaryShape.Pile:
                field = DrawLineSegment(field, new Vector2Int(field.width / 4, field.height - 1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                field = DrawLineSegment(field, new Vector2Int(field.width * 3 / 4, field.height - 1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                field = FloodFill(field, new Vector2Int(field.width / 2, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                return field;

            case OrdinaryShape.Chevron:
                Color[,] stamp = field.GetStampTemplate();
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(0, field.height / 4), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(field.width - 1, field.height / 4), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(0, field.height / 4 - field.height / 5), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(field.width - 1, field.height / 4 - field.height / 5), tincture);
                stamp = FloodFill(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - 1), tincture, FloodFillMode.UntilTargetColor);

                field = StampToTexture(field, stamp);
                return field;

            case OrdinaryShape.Pall:
            case OrdinaryShape.Bordure:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", shape);
                return field;

        }
    }

    public static Texture2D ApplyCharge(Texture2D field, Charge charge) {
        switch (charge.type) {
            case ChargeType.Single:
                charge.point = Point.FessPoint;
                return ApplyChargeAtPoint(field, charge, field.width / 2);

            case ChargeType.Positioned:
                return ApplyChargeAtPoint(field, charge, field.width / 3);

            case ChargeType.Multiple:
                RectInt[] placements = GetChargeLayout(field, charge.count);
                return ApplyChargesAtPlacements(field, charge, placements);

            case ChargeType.LaidOut:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.type);
                return field;
        }
    }

    private static Texture2D ApplyChargeAtPoint(Texture2D field, Charge charge, int scale) {
        RectInt outerRect = RectFromPoint(field, charge.point, scale);
        outerRect = outerRect.ConcentricRectInt(field.width / 32);
        switch (charge.shape) {
            case ChargeShape.Roundel:
                field = DrawCircle(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Lozenge:
                field = DrawLozenge(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Fusil:
                field = DrawFusil(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Billet:
                field = DrawBillet(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Annulet:
                RectInt innerRect = outerRect.ConcentricRectInt(outerRect.width / 4);
                field = DrawAnnulet(field, outerRect, innerRect, charge.tincture);
                return field;
            case ChargeShape.Mascle:
                RectInt innerRect2 = outerRect.ConcentricRectInt(outerRect.width / 4);
                field = DrawMascle(field, outerRect, innerRect2, charge.tincture);
                return field;
            case ChargeShape.Rustre:
                field = DrawRustre(field, outerRect, charge.tincture);
                return field;
            case ChargeShape.Mullet:
                field = DrawMullet(field, outerRect, charge.tincture, 5);
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.shape);
                return field;
        }
    }

    public static Texture2D ApplyChargesAtPlacements(Texture2D field, Charge charge, RectInt[] placements) {
        switch (charge.shape) {
            case ChargeShape.Roundel:
                foreach (RectInt place in placements) {
                    field = DrawCircle(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Lozenge:
                foreach (RectInt place in placements) {
                    field = DrawLozenge(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Fusil:
                foreach (RectInt place in placements) {
                    field = DrawFusil(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Billet:
                foreach (RectInt place in placements) {
                    field = DrawBillet(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Annulet:
                foreach (RectInt place in placements) {
                    RectInt innerRect = place.ConcentricRectInt(place.width / 4);
                    field = DrawAnnulet(field, place, innerRect, charge.tincture);
                }
                return field;
            case ChargeShape.Mascle:
                foreach (RectInt place in placements) {
                    RectInt innerRect2 = place.ConcentricRectInt(place.width / 4);
                    field = DrawMascle(field, place, innerRect2, charge.tincture);
                }
                return field;
            case ChargeShape.Rustre:
                foreach (RectInt place in placements) {
                    field = DrawRustre(field, place, charge.tincture);
                }
                return field;
            case ChargeShape.Mullet:
                foreach (RectInt place in placements) {
                    field = DrawMullet(field, place, charge.tincture, 5);
                }
                return field;
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.shape);
                return field;
        }
    }

    #endregion

    #region Helper Functions

    public static RectInt RectFromPoint(Texture2D field, Point point, int size) {
        return point switch {
            Point.Chief => new RectInt(field.width / 2 - size / 2, field.height - 1, size, size),
            Point.Dexter => new RectInt(0, field.height / 2 + size / 2, size, size),
            Point.Sinister => new RectInt(field.width - size, field.height / 2 + size / 2, size, size),
            Point.Base => new RectInt(field.width / 2 - size / 2, size, size, size),

            Point.DexterChief => new RectInt(0, field.height - 1, size, size),
            Point.MiddleChief => new RectInt(field.width / 2 - size / 2, field.height - 1, size, size),
            Point.SinisterChief => new RectInt(field.width - size, field.height - 1, size, size),

            Point.DexterBase => new RectInt(0, size, size, size),
            Point.MiddleBase => new RectInt(field.width / 2 - size / 2, size, size, size),
            Point.SinisterBase => new RectInt(field.width - size, size, size, size),

            Point.FessPoint => new RectInt(field.width / 2 - size / 2, field.height / 2 + size / 2, size, size),
            // Half way between the Middle Chief and the Fess Point
            Point.HonourPoint => new RectInt(field.width / 2 - size / 2, (field.height - 1 + field.height / 2 + size / 2) / 2, size, size),
            //Halfway between the Base and the Honor Point
            Point.NombrilPoint => new RectInt(field.width / 2 - size / 2, ((field.height - 1 + field.height / 2 + size / 2) / 2 + size) / 2, size, size),
            //Default to the Fess Point
            _ => new RectInt(field.width / 2 - size / 2, field.height / 2 + size / 2, size, size),
        };
    }

    public static RectInt[] GetChargeLayout(Texture2D field, int number) {
        if(number < 1) {
            throw new ArgumentException("Number of charges must be greater than 0");
        }
        int width;
        switch (number) {
            case 1:
                return new RectInt[] { RectFromPoint(field, Point.FessPoint, field.width / 2) };
            case 2:
                width = field.width / 4;
                return new RectInt[] { new RectInt(field.width / 7, field.height * 3 / 4 + width / 2, width, width),
                            new RectInt(field.width * 4 / 7, field.height * 3 / 4 + width / 2, width, width) };
            case 4:
                width = field.width / 4;
                int x1 = field.width / 3;
                int x2 = field.width * 2 / 3;
                int y1 = field.height * 2 / 5;
                int y2 = field.height * 4 / 5;
                return new RectInt[] { new RectInt(x1 - width / 2, y1 + width / 2, width, width),
                                        new RectInt(x1 - width / 2, y2 + width / 2, width, width),
                                        new RectInt(x2 - width / 2, y1 + width / 2, width, width),
                                        new RectInt(x2 - width / 2, y2 + width / 2, width, width) };
                  
            default:
                width = field.width / Mathf.Max(number, 4);
                int angle = 360 / number;
                List<RectInt>  rects = new List<RectInt>();
                //iterate through the number of charge moving along a circle, starting at the top generate a point and then rotate the point by the angle
                //add the point to the list of points
                for (int i = 0; i < number; i++) {
                        int x = field.width / 2 + (int)((field.width / 4) * Mathf.Sin((angle * i + 180) * Mathf.Deg2Rad));
                        int y = field.height / 2 + (int)((field.height / 4) * Mathf.Cos((angle * i + 180) * Mathf.Deg2Rad));
                        rects.Add(new RectInt(x - width / 2, y + width / 2, width, width));
                }
                return rects.ToArray();
        }
    }

    public static Texture2D TextureFromColorsArray(Color[,] colors) {
        Texture2D tex = new Texture2D(colors.GetLength(0), colors.GetLength(1));
        Color[] pix = new Color[colors.GetLength(0) * colors.GetLength(1)];
        for (int x = 0; x < colors.GetLength(0); x++) {
            for (int y = 0; y < colors.GetLength(1); y++) {
                pix[x + y * colors.GetLength(0)] = colors[x, y];
            }
        }
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    #endregion

    #region Primitive Drawing Functions

    public static Texture2D Fill(Texture2D field, Color color) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                field.SetPixel(x, y, color);
            }
        }
        return field;
    }

    public static Texture2D Replace(Texture2D field, Color oldColor, Color targetColor) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (field.GetPixel(x, y) == oldColor) {
                    field.SetPixel(x, y, targetColor);
                }
            }
        }
        return field;
    }

    public static Texture2D StampToTexture (Texture2D field, Color[,] stamp) {
        if(field.width != stamp.GetLength(0) || field.height != stamp.GetLength(1)) {
            throw new ArgumentException("Stamp and field must be the same size");
        }

        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (stamp[x, y] != Color.clear) {
                    field.SetPixel(x, y, stamp[x, y]);
                }
            }
        }
        return field;
    }

    public static Texture2D CutToStamp(Texture2D field, Color[,] stamp) {
        if (field.width != stamp.GetLength(0) || field.height != stamp.GetLength(1)) {
            throw new ArgumentException("Stamp and field must be the same size");
        }
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (stamp[x, y].a == 0) {
                    field.SetPixel(x, y, Color.clear);
                }
            }
        }
        return field;
    }

    public static Texture2D DrawRect(Texture2D field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field.SetPixel(x, y, color);
            }
        }
        return field;
    }

    public static Color[,] DrawRect(Color[,] field, RectInt rect, Color color) {
        for (int x = rect.x; x < rect.x + rect.width; x++) {
            for (int y = rect.y; y < rect.y + rect.height; y++) {
                field[x, y] = color;
            }
        }
        return field;
    }

    public static Texture2D DrawBillet(Texture2D field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        rect.y -= rect.height;
        return DrawRect(field, rect, color);
    }

    public static Color[,] DrawBillet(Color[,] field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        rect.y -= rect.height;
        return DrawRect(field, rect, color);
    }

    public static Texture2D DrawLozenge(Texture2D field, RectInt rect, Color color) {
        Color[,] stamp = field.GetColorArray();
        stamp = DrawLozenge(stamp, rect, color);
        field = StampToTexture(field, stamp);
        return field;
    }

    public static Color[,] DrawLozenge(Color[,] field, RectInt rect, Color color) {
        Vector2Int left = new Vector2Int(rect.x, rect.y - rect.height / 2);
        Vector2Int right = new Vector2Int(rect.x + rect.width, rect.y - rect.height / 2);
        Vector2Int top = new Vector2Int(rect.x + rect.width / 2, rect.y);
        Vector2Int bottom = new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height);
        field = DrawLineSegment(field, left, top, color);
        field = DrawLineSegment(field, top, right, color);
        field = DrawLineSegment(field, right, bottom, color);
        field = DrawLineSegment(field, bottom, left, color);
        field = FloodFill(field, new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height / 2), color, FloodFillMode.UntilTargetColor);
        return field;
    }

    public static Texture2D DrawFusil(Texture2D field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        return DrawLozenge(field, rect, color);
    }

    public static Color[,] DrawFusil(Color[,] field, RectInt rect, Color color) {
        rect.x += rect.width / 4;
        rect.width = rect.width / 2;
        return DrawLozenge(field, rect, color);
    }

    public static Texture2D DrawMascle(Texture2D field, RectInt outerRect, RectInt innerRect, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawLozenge(stamp, outerRect, color);
        stamp = DrawLozenge(stamp, innerRect, Color.clear);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawRustre(Texture2D field, RectInt outerRect, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawFusil(stamp, outerRect, color);
        stamp = DrawCircle(stamp, new Vector2Int(outerRect.x + outerRect.width / 4,
                                                 outerRect.y + outerRect.height / 4),
                                                 outerRect.width / 4, Color.clear);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawMullet(Texture2D field, RectInt outerRect, Color color, int points=5) {
        Color[,] stamp = field.GetStampTemplate();

        Vector2Int rectCenter = new Vector2Int(outerRect.x + outerRect.width / 2, 
                                               outerRect.y - outerRect.height / 2);

        int outerRadius = outerRect.width / 2;
        int innerRadius = outerRadius * 3 / 8;
        int angle = 360 / points / 2;
        bool innerPoint = true;

        Vector2Int lastPoint = rectCenter + new Vector2Int(0, outerRadius);
        Vector2Int currPoint;
        for(int i = 1; i < points * 2; i++) {
            if (innerPoint) {
                currPoint = rectCenter + new Vector2Int((int)(innerRadius * Mathf.Sin((angle * i) * Mathf.Deg2Rad)),
                                                        (int)(innerRadius * Mathf.Cos((angle * i) * Mathf.Deg2Rad)));
                stamp = DrawLineSegment(stamp, lastPoint, currPoint, color);
                lastPoint = currPoint;
                innerPoint = false;
            }
            else {
                currPoint = rectCenter + new Vector2Int((int)(outerRadius * Mathf.Sin((angle * i) * Mathf.Deg2Rad)),
                                                        (int)(outerRadius * Mathf.Cos((angle * i) * Mathf.Deg2Rad)));
                stamp = DrawLineSegment(stamp, lastPoint, currPoint, color);
                lastPoint = currPoint;
                innerPoint = true;
            }
        }
        stamp = DrawLineSegment(stamp, lastPoint, rectCenter + new Vector2Int(0, outerRadius), color);
        stamp = FloodFill(stamp, rectCenter, color, FloodFillMode.UntilTargetColor);
        return StampToTexture(field, stamp);
    }

    public static Texture2D DrawCircle(Texture2D field, RectInt rect, Color color) {
        return DrawCircle(field, new Vector2Int(rect.x + rect.width / 2, rect.y - rect.height / 2), rect.width / 2, color);
    }

    public static Texture2D DrawCircle(Texture2D field, Vector2Int center, int radius, Color color) {
        for (int x = center.x - radius; x < center.x + radius; x++) {
            for (int y = center.y - radius; y < center.y + radius; y++) {
                if (Vector2Int.Distance(center, new Vector2Int(x, y)) <= radius) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    public static Color[,] DrawCircle(Color[,] field, Vector2Int center, int radius, Color color) {
        for (int x = center.x - radius; x < center.x + radius; x++) {
            for (int y = center.y - radius; y < center.y + radius; y++) {
                if (Vector2Int.Distance(center, new Vector2Int(x, y)) <= radius) {
                    field[x, y] = color;
                }
            }
        }
        return field;
    }

    public static Texture2D DrawAnnulet(Texture2D field, RectInt outerRect, RectInt innerRect, Color color) {
        Debug.LogFormat("DrawAnnulet: innerRect: {0}, OuterRect: {1}", innerRect, outerRect);    
        return DrawAnnulet(field, new Vector2Int(outerRect.x + outerRect.width / 2, outerRect.y - outerRect.height / 2),  innerRect.width / 2, outerRect.width / 2, color);
    }

    public static Texture2D DrawAnnulet(Texture2D field, Vector2Int center, int minRadius, int maxRadius, Color color) {
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawCircle(stamp, center, maxRadius, color);
        stamp = DrawCircle(stamp, center, minRadius, Color.clear);
        field = StampToTexture(field, stamp);
        return field;
    }
    //Given a Color[,] field and starting point, draw a line in the field of a given slope using Bresenham's line algorithm
    
    public static Texture2D DrawLine(Texture2D field, Vector2Int intercept, Vector2Int slope, Color color, int thickness) {

        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (slope.x * (y - intercept.y) == slope.y * (x - intercept.x)) {
                    if (thickness == 1) {
                        field.SetPixel(x, y, color);
                    }
                    else {
                        for (int i = -thickness / 2; i < thickness / 2; i++) {
                            for (int j = -thickness / 2; j < thickness / 2; j++) {
                                if (x + i < field.width && y + j < field.height &&
                                    x + i >= 0 && y + j >= 0) {
                                    field.SetPixel(x + i, y + j, color);
                                }
                            }
                        }
                    }
                }
            }
        }
        return field;
    }

    public static Color[,] DrawLine(Color[,] field, Vector2Int intercept, Vector2Int slope, Color color, int thickness) {

        for (int x = 0; x < field.GetLength(0); x++) {
            for (int y = 0; y < field.GetLength(1); y++) {
                if (slope.x * (y - intercept.y) == slope.y * (x - intercept.x)) {
                    if (thickness == 1) {
                        field[x, y] = color;
                    }
                    else {
                        for (int i = -thickness / 2; i < thickness / 2; i++) {
                            for (int j = -thickness / 2; j < thickness / 2; j++) {
                                if (x + i < field.GetLength(0) && y + j < field.GetLength(1) &&
                                    x + i >= 0 && y + j >= 0) {
                                    field[x + i, y + j] = color;
                                }
                            }
                        }
                    }
                }
            }
        }
        return field;
    }

    public static Texture2D DrawLineSegment(Texture2D field, Vector2Int start, Vector2Int end, Color color) {
        int dx = Mathf.Abs(end.x - start.x);
        int sx = start.x < end.x ? 1 : -1;
        int dy = -Mathf.Abs(end.y - start.y);
        int sy = start.y < end.y ? 1 : -1;
        int err = dx + dy;

        while (true) {
            field.SetPixel(start.x, start.y, color);
            if (start.x == end.x && start.y == end.y) {
                break;
            }

            int e2 = 2 * err;

            if (e2 >= dy) {
                if (start.x == end.x) {
                    break;
                }
                err += dy;
                start.x += sx;
            }
            if (e2 <= dx) {
                if (start.y == end.y) {
                    break;
                }
                err += dx;
                start.y += sy;
            }
        }
        return field;
    }

    public static Color[,] DrawLineSegment(Color[,] field, Vector2Int start, Vector2Int end, Color color) {
        int dx = Mathf.Abs(end.x - start.x);
        int sx = start.x < end.x ? 1 : -1;
        int dy = -Mathf.Abs(end.y - start.y);
        int sy = start.y < end.y ? 1 : -1;
        int err = dx + dy;

        while (true) {
            field[start.x, start.y] = color;
            if (start.x == end.x && start.y == end.y) {
                break;
            }

            int e2 = 2 * err;

            if (e2 >= dy) {
                if (start.x == end.x) {
                    break;
                }
                err += dy;
                start.x += sx;
            }
            if (e2 <= dx) {
                if (start.y == end.y) {
                    break;
                }
                err += dx;
                start.y += sy;
            }
        }
        return field;
    }

    public enum FloodFillMode {
        AllSeedColor,
        UntilTargetColor
    }

    public static Texture2D FloodFill(Texture2D field, Vector2Int seed, Color color, FloodFillMode fillMode) {
        Color seedColor = field.GetPixel(seed.x, seed.y);
        if (fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field.SetPixel(node.x, node.y, color);

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field.GetPixel(node.x - 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node.x + 1, node.y) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field.GetPixel(node.x, node.y - 1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node.x, node.y + 1) == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                case FloodFillMode.UntilTargetColor:
                    if (node.x > 0 && field.GetPixel(node.x - 1, node.y) != color) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.width - 1 && field.GetPixel(node.x + 1, node.y) != color) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field.GetPixel(node.x, node.y - 1) != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.height - 1 && field.GetPixel(node.x, node.y + 1) != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                default:
                    goto case FloodFillMode.AllSeedColor;
            }
        }
        return field;

    }

    public static Color[,] FloodFill(Color[,] field, Vector2Int seed, Color color, FloodFillMode fillMode) {
        Color seedColor = field[seed.x, seed.y];
        if (fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field[node.x, node.y] = color;

            switch (fillMode) {
                case FloodFillMode.AllSeedColor:
                    if (node.x > 0 && field[node.x - 1, node.y] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.GetLength(0) - 1 && field[node.x + 1, node.y] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field[node.x, node.y - 1] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.GetLength(1) - 1 && field[node.x, node.y + 1] == seedColor) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                case FloodFillMode.UntilTargetColor:
                    if (node.x > 0 && field[node.x - 1, node.y] != color) {
                        pixelStack.Push(new Vector2Int(node.x - 1, node.y));
                    }
                    if (node.x < field.GetLength(0) - 1 && field[node.x + 1, node.y] != color) {
                        pixelStack.Push(new Vector2Int(node.x + 1, node.y));
                    }
                    if (node.y > 0 && field[node.x, node.y - 1] != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y - 1));
                    }
                    if (node.y < field.GetLength(1) - 1 && field[node.x, node.y + 1] != color) {
                        pixelStack.Push(new Vector2Int(node.x, node.y + 1));
                    }
                    break;
                default:
                    goto case FloodFillMode.AllSeedColor;
            }
        }
        return field;

    }

    #endregion

}
