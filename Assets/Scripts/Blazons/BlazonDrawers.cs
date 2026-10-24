using System;
using System.Collections.Generic;
using UnityEngine;
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

        switch (variation) {
            case Variation.Paly:
                // band index comes from position alone, so counts that don't divide the width stay clean
                for (int x = 0; x < field.width; x++) {
                    int band = x * patternCount / field.width;
                    for (int y = 0; y < field.height; y++) {
                        field.SetPixel(x, y, tinctures[(band + 1) % tinctures.Length]);
                    }
                }
                return field;
            case Variation.Barry:
                for (int y = field.height - 1; y >= 0; y--) {
                    int band = (field.height - 1 - y) * patternCount / field.height;
                    for (int x = 0; x < field.width; x++) {
                        field.SetPixel(x, y, tinctures[band % tinctures.Length]);
                    }
                }
                return field;
            case Variation.Bendy:
                if (sinister) {
                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            // band index depends only on the diagonal this pixel sits on
                            int band = (x + (field.width - 1 - y)) * patternCount / field.width;
                            field.SetPixel(x, y, tinctures[(band + 1) % tinctures.Length]);
                        }
                    }
                }
                else {
                    for (int y = field.height - 1; y >= 0; y--) {
                        for (int x = 0; x < field.width; x++) {
                            int band = (x + y + 1) * patternCount / field.width;
                            field.SetPixel(x, y, tinctures[(band + 1) % tinctures.Length]);
                        }
                    }
                }
                return field;
            case Variation.Chequy:
                for (int x = 0; x < field.width; x++) {
                    for (int y = 0; y < field.height; y++) {
                        if ((x * patternCount / field.width + y * patternCount / field.height) % 2 == 0) {
                            field.SetPixel(x, y, tinctures[1]);
                        }
                        else {
                            field.SetPixel(x, y, tinctures[0]);
                        }
                    }
                }
                return field;
            case Variation.Chevronny:
                for (int y = field.height - 1; y >= 0; y--) {
                    for (int x = 0; x < field.width / 2; x++) {
                        int band = (x + (field.width - 1 - y)) * patternCount / field.width;
                        field.SetPixel(x, y, tinctures[(band + 1) % tinctures.Length]);
                    }
                    for (int x = field.width / 2; x < field.width; x++) {
                        int band = (x + y + 1) * patternCount / field.width;
                        field.SetPixel(x, y, tinctures[band % tinctures.Length]);
                    }
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

    public static Texture2D ApplyOrdinary(Texture2D field, OrdinaryShape shape, Color tincture, bool sinister = false, Shield shield = null) {
        Color [,] stamp = field.GetStampTemplate();
        switch (shape) {
            case OrdinaryShape.Chief:
                field = DrawRect(field, new RectInt(0, field.height * 2 / 3, field.width, field.height), tincture);
                return field;
            case OrdinaryShape.Fess:
                field = DrawRect(field, new RectInt(0, field.height / 2 - field.height / 8, field.width, field.height / 4), tincture);
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

                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 8, 0), new Vector2Int(field.width - 1, field.height * 7 / 8), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(0, field.height / 8), new Vector2Int(field.width * 7 / 8, field.height - 1), tincture);

                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 8, field.height - 1), new Vector2Int(field.width - 1, field.height / 8), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(0, field.width * 7 / 8), new Vector2Int(field.width * 7 / 8, 0), tincture);

                stamp = FloodFill(stamp, new Vector2Int(field.width / 2, field.height / 2), tincture, FloodFillMode.UntilTargetColor);
                stamp = FloodFill(stamp, new Vector2Int(0, 0), tincture, FloodFillMode.UntilTargetColor);
                stamp = FloodFill(stamp, new Vector2Int(0, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                stamp = FloodFill(stamp, new Vector2Int(field.width - 1, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                stamp = FloodFill(stamp, new Vector2Int(field.width - 1, 0), tincture, FloodFillMode.UntilTargetColor);
                return StampToTexture(field, stamp);

            case OrdinaryShape.Pile:
                stamp = field.GetStampTemplate();
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 4, field.height - 1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width * 3 / 4, field.height - 1), new Vector2Int(field.width / 2, field.height / 10), tincture);
                stamp = FloodFill(stamp, new Vector2Int(field.width / 2, field.height - 1), tincture, FloodFillMode.UntilTargetColor);
                return StampToTexture(field, stamp);

            case OrdinaryShape.Chevron:
                stamp = field.GetStampTemplate();
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(0, field.height / 4), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4), new Vector2Int(field.width - 1, field.height / 4), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(0, field.height / 4 - field.height / 5), tincture);
                stamp = DrawLineSegment(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - field.height / 5), new Vector2Int(field.width - 1, field.height / 4 - field.height / 5), tincture);
                stamp = FloodFill(stamp, new Vector2Int(field.width / 2, field.height * 3 / 4 - 1), tincture, FloodFillMode.UntilTargetColor);

                field = StampToTexture(field, stamp);
                return field;

            case OrdinaryShape.Bordure:
                return DrawBordure(field, tincture, shield);

            case OrdinaryShape.Pall:
                return DrawPall(field, tincture, shield);

            default:
                Debug.LogFormat("Haven't implemented {0} yet", shape);
                return field;

        }
    }

    public static Texture2D ApplyCharge(Texture2D field, Charge charge, Shield shield = null) {
        shield = shield ?? Shield.Default;
        int shieldWidth = shield.Bounds(field.width).width;
        switch (charge.type) {
            case ChargeType.Single:
                charge.point = Point.FessPoint;
                return ApplyChargeAtPoint(field, charge, shieldWidth / 2, shield);

            case ChargeType.Positioned:
                return ApplyChargeAtPoint(field, charge, shieldWidth / 3, shield);

            case ChargeType.Multiple:
                RectInt[] placements = GetChargeLayout(field, charge.count, shield);
                return ApplyChargesAtPlacements(field, charge, placements);

            case ChargeType.LaidOut:
            default:
                Debug.LogFormat("Haven't implemented {0} yet", charge.type);
                return field;
        }
    }

    private static Texture2D ApplyChargeAtPoint(Texture2D field, Charge charge, int scale, Shield shield) {
        RectInt outerRect = RectFromPoint(field, charge.point, scale, shield);
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

    // Where a charge of `size` pixels goes at a named point. The points belong to the shield, see Shield.PlaceCharge.
    public static RectInt RectFromPoint(Texture2D field, Point point, int size, Shield shield = null) {
        return (shield ?? Shield.Default).PlaceCharge(point, size, field.width);
    }

    // Where each of `number` charges goes, laid out within the shield, see Shield.PlaceCharges.
    public static RectInt[] GetChargeLayout(Texture2D field, int number, Shield shield = null) {
        return (shield ?? Shield.Default).PlaceCharges(number, field.width);
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
        Color[,] stamp = field.GetStampTemplate();
        stamp = DrawLozenge(stamp, rect, color);
        return StampToTexture(field, stamp);
    }

    public static Color[,] DrawLozenge(Color[,] field, RectInt rect, Color color) {
        float slope = (float)rect.height / rect.width;
        for(int x = 0; x < rect.width; x++) {
            for(int y = 0; y < rect.height; y++) {                
                
                if(x < rect.width/2) {
                    //we're left
                    if( y > rect.height / 2) {
                        //Up left
                        if(y <= x * slope + rect.height / 2 && 
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) && 
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0 ) { 
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                    else {
                        //Down Left
                        if(y >= x * -slope + rect.height /2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                }
                else {
                    if (y > rect.height / 2) {
                        // Up right
                        if (y <= x * -slope + rect.height * 3 / 2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                    else {
                        // Down right
                        if (y >= x * slope - rect.height / 2 &&
                            x + rect.x < field.GetLength(0) && y + rect.y - rect.height < field.GetLength(1) &&
                            x + rect.x >= 0 && y + rect.y - rect.height >= 0) {
                            field[x + rect.x, y + rect.y - rect.height] = color;
                        }
                    }
                }
            }
        }
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
        stamp = DrawCircle(stamp, new Vector2Int(outerRect.x + outerRect.width / 2,
                                                 outerRect.y - outerRect.height / 2),
                                                 outerRect.width / 8, Color.clear);
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
    
    public static Texture2D DrawSinWave(Texture2D field, RectInt rect, Color color, float amplitude, float frequency, float phase) {
        for (int x = 0; x < rect.width; x++) {
            for(int y = 0; y < rect.height; y++) {
                if(y < (int)(rect.height / 2 + amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad))) {
                    field.SetPixel(rect.x + x, rect.height + y, color);
                }
            }
            //field.SetPixel(x, (int)(rect.y - amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)), color);
        }
        return field;
    }

    // Rotates the pixels of the texture about its centre, counter clockwise for positive angles.
    // Corners that rotate in from outside the texture become Color.clear. To rotate just one shape, draw it into a
    // blank stamp (GetStampTemplate), RotateStamp that and StampToTexture it instead.
    public static Texture2D RotateTexture(Texture2D field, float angle) {
        Color[,] rotated = RotateStamp(field.GetColorArray(), angle);
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                field.SetPixel(x, y, rotated[x, y]);
            }
        }
        return field;
    }

    // Rotates a stamp about its centre, counter clockwise for positive angles. Pixels with nothing behind them are Color.clear.
    // Works backwards from each destination pixel to the source pixel it came from, so the result has no holes at any angle.
    public static Color[,] RotateStamp(Color[,] stamp, float angle) {
        int width = stamp.GetLength(0);
        int height = stamp.GetLength(1);
        Color[,] ret = new Color[width, height];

        float ang = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(ang);
        float sin = Mathf.Sin(ang);
        float cx = width / 2f;
        float cy = height / 2f;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // centre of the destination pixel relative to the middle, rotated back by the angle
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                int sx = Mathf.FloorToInt(cos * dx + sin * dy + cx);
                int sy = Mathf.FloorToInt(-sin * dx + cos * dy + cy);

                if (sx >= 0 && sx < width && sy >= 0 && sy < height) {
                    ret[x, y] = stamp[sx, sy];
                }
            }
        }
        return ret;
    }

    public static Texture2D DrawScallop(Texture2D field, RectInt rect, Color color, float amplitude, float frequency, float phase, bool up) {
        for (int x = 0; x < rect.width; x++) {
            for (int y = 0; y < rect.height; y++) {
                if (up) {
                    if (y < (int)(rect.height / 2 + amplitude * Mathf.Abs(Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)))) {
                        field.SetPixel(rect.x + x, rect.height + y, color);
                    }
                }
                else {
                    if (y < (int)(rect.height / 2 - amplitude * Mathf.Abs(Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)))) {
                        field.SetPixel(rect.x + x, rect.height + y, color);
                    }
                }
            }
            //field.SetPixel(x, (int)(rect.y - amplitude * Mathf.Sin((x + phase) * frequency * Mathf.Deg2Rad)), color);
        }
        return field;
    }

    #region Wave Drawing Functions

    // Fills everything on `side` of a line bent by a profile. The line types are in Line.cs.
    public static Texture2D DrawWave(Texture2D field, WaveLine line, Color color, LineSide side = LineSide.Right) {
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (line.IsOnSide(x, y, side)) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    // Same, into a stamp. Draw with Color.clear to cut a wavy edge out of a stamp.
    public static Color[,] DrawWave(Color[,] field, WaveLine line, Color color, LineSide side = LineSide.Right) {
        for (int x = 0; x < field.GetLength(0); x++) {
            for (int y = 0; y < field.GetLength(1); y++) {
                if (line.IsOnSide(x, y, side)) {
                    field[x, y] = color;
                }
            }
        }
        return field;
    }

    public static Texture2D DrawSinWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Sine(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawSinWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Sine(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawSquareWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Square(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawSquareWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Square(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawTriangleWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Triangle(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawTriangleWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Triangle(), amplitude, wavelength, phase), color, side);
    }

    public static Texture2D DrawScallopWave(Texture2D field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Scallop(), amplitude, wavelength, phase), color, side);
    }

    public static Color[,] DrawScallopWave(Color[,] field, Vector2 origin, float angle, Color color, float amplitude, float wavelength, float phase = 0f, LineSide side = LineSide.Right) {
        return DrawWave(field, new WaveLine(origin, angle, LineProfile.Scallop(), amplitude, wavelength, phase), color, side);
    }

    #endregion

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

    // Height of the pall's junction as a fraction of the shield's height, same as the pall division uses.
    private const float PallJunctionHeight = 2f / 3f;

    // Band about 1/6 of the shield's width running around the inside of the shield outline, whatever shape it is.
    // Paints everything outside the inset shield, including past the outline, so it reaches the texture edge
    // like the other ordinaries do. Add `&& shield.Contains(x, y, field.width)` to keep it strictly inside the shield.
    public static Texture2D DrawBordure(Texture2D field, Color color, Shield shield = null) {
        shield = shield ?? Shield.Default;
        float width = shield.Bounds(field.width).width / 6f;
        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                if (shield.DistanceToEdge(x, y, field.width) <= width) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    // Y shape: arms from each upper corner of the shield meet at a junction, then one arm runs down to the bottom middle.
    // Each arm is about 1/6 of the shield's width thick.
    public static Texture2D DrawPall(Texture2D field, Color color, Shield shield = null) {
        shield = shield ?? Shield.Default;
        RectInt bounds = shield.Bounds(field.width);
        float thickness = bounds.width / 6f;
        Vector2 junction = new Vector2(bounds.x + bounds.width / 2f, bounds.y + bounds.height * PallJunctionHeight);

        // arms run past the edge of the shield so no end caps show
        Vector2 dexterEnd = junction + (new Vector2(bounds.xMin, bounds.yMax - 1) - junction) * 1.25f;
        Vector2 sinisterEnd = junction + (new Vector2(bounds.xMax - 1, bounds.yMax - 1) - junction) * 1.25f;
        Vector2 baseEnd = new Vector2(junction.x, bounds.yMin - thickness);

        for (int x = 0; x < field.width; x++) {
            for (int y = 0; y < field.height; y++) {
                Vector2 p = new Vector2(x, y);
                float d = Mathf.Min(DistanceToSegment(p, junction, dexterEnd),
                           Mathf.Min(DistanceToSegment(p, junction, sinisterEnd),
                                     DistanceToSegment(p, junction, baseEnd)));
                if (d <= thickness / 2f) {
                    field.SetPixel(x, y, color);
                }
            }
        }
        return field;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b) {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
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
        color = (Color32)color;   // compare against what the 8-bit texture actually stores
        Color seedColor = field.GetPixel(seed.x, seed.y);
        if (fillMode == FloodFillMode.AllSeedColor && seedColor == color) {
            return field;
        }
        Stack<Vector2Int> pixelStack = new Stack<Vector2Int>();
        pixelStack.Push(seed);
        while (pixelStack.Count > 0) {
            Vector2Int node = pixelStack.Pop();
            field.SetPixel(node.x, node.y, color);

            if(pixelStack.Count > field.width * field.height) {
                Debug.LogWarningFormat("FloodFill aborted: stack exceeded {0} entries (seed {1}, color {2}, mode {3}). The fill is probably not terminating.", field.width * field.height, seed, color, fillMode);
                return field;
            }

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
