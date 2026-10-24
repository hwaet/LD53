using System.Collections.Generic;
using static Charge;
using static Field;
using static Ordinary;
using static Blazon;

public static class BlazonParserHelpers {

    public static readonly Dictionary<string, Variation> Variations = new Dictionary<string, Variation>() {
        {"barry", Variation.Barry },
        {"paly", Variation.Paly },
        {"bendy",Variation.Bendy },
        {"chevronny", Variation.Chevronny },
        {"chequy", Variation.Chequy },
        {"lozengy", Variation.Lozengy },
        {"fusilly", Variation.Fusilly },
        {"gyronny", Variation.Gyronny }
    };

    public static readonly Dictionary<string, Division> Divisions = new Dictionary<string, Division>() {
        {"fess", Division.Fess },
        {"pale", Division.Pale },
        {"bend", Division.Bend },
        {"chevron", Division.Chevron },
        {"cross", Division.Cross },
        {"saltire", Division.Saltire },
        {"pall", Division.Pall }
    };

    public static readonly HashSet<string> DivisionCues = new HashSet<string>() {
        "party"," parted", "per"
    };

    public static readonly Dictionary<string, ChargeShape> ChargeShapes = new Dictionary<string, ChargeShape>() {
        {"roundel", ChargeShape.Roundel},
        {"lozenge", ChargeShape.Lozenge },
        {"fusil", ChargeShape.Fusil },
        {"billet", ChargeShape.Billet },
        {"mascle", ChargeShape.Mascle },
        {"annulet", ChargeShape.Annulet },
        {"rustre", ChargeShape.Rustre },
        {"mullet", ChargeShape.Mullet },

        {"roundels", ChargeShape.Roundel},
        {"lozenges", ChargeShape.Lozenge },
        {"fusils", ChargeShape.Fusil },
        {"billets", ChargeShape.Billet },
        {"mascles", ChargeShape.Mascle },
        {"annulets", ChargeShape.Annulet },
        {"rustres", ChargeShape.Rustre },
        {"mullets", ChargeShape.Mullet }
    };

    public static readonly Dictionary<string, OrdinaryShape> OrdinaryShapes = new Dictionary<string, OrdinaryShape>() {
        {"cross", OrdinaryShape.Cross },
        {"pale", OrdinaryShape.Pale },
        {"fess", OrdinaryShape.Fess },
        {"bend", OrdinaryShape.Bend },
        {"chevron", OrdinaryShape.Chevron },
        {"saltire", OrdinaryShape.Saltire },
        {"chief", OrdinaryShape.Chief },
        {"bordure", OrdinaryShape.Bordure },
        {"pile", OrdinaryShape.Pile },
        {"pall", OrdinaryShape.Pall }
    };

    public static readonly Dictionary<string, int> Numbers = new Dictionary<string, int>() {
        {"one", 1 },
        {"two", 2},
        {"three", 3},
        {"four", 4},
        {"five", 5 },
        {"six", 6 },
        {"seven", 7 },
        {"eight", 8 },
        {"nine", 9 },
        {"ten", 10 },
        {"eleven", 11 },
        {"twelve", 12 },
        {"thirteen", 13 },
        {"fourteen", 14 },
        {"fifteen", 15 }
    };

    public static readonly Dictionary<string, Point> Points = new Dictionary<string, Point>() {
        {"chief", Point.Chief},
        {"dexter", Point.Dexter },
        {"sinister", Point.Sinister },
        {"base", Point.Base },
        {"dexter chief", Point.DexterChief },
        {"middle chief", Point.MiddleChief },
        {"sinister chief", Point.SinisterChief },
        {"honour point", Point.HonourPoint },
        {"fess point", Point.FessPoint },
        {"nombril point", Point.NombrilPoint },
        {"dexter base", Point.DexterBase },
        {"middle base", Point.MiddleBase },
        {"sinister base", Point.SinisterBase }
    };

}