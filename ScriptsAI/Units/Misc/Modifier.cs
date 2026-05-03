using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase singleton para calcular guardar los modificadores y calcular el modificador para cada situaci n
public sealed class Modifier
{
    private double[,] AM;  // Modificador de Ataque (Attack modifier)
    private double[,] PTM; // Modificator precisi n por terreno (Precision terrain modifier)
    private double[,] DTM; // Modificador esquiva por terreno (Dodge terrain modifier)
    private double[,] MTM; // Modificador movimiento por terreno (Movement terrain modifier)

    private static Modifier _instance;


    // Constructor de la clase
    private Modifier()
    {
        AM = new double[,]
        {
            {1,     0.75,   1.5,    1.25,   1.25,   1.25,   1.5 },
            {1.5,   1,      0.75,   1.25,   1.25,   1.25,   1.5 },
            {0.75,  1.5,    1,      1.25,   1.25,   1.25,   1.5 },
            {1.25,  1.25,   1.25,   1,      0.75,   1.5,    1.5 },
            {1.25,  1.25,   1.25,   1.5,    1,      0.75,   1.5 },
            {1.25,  1.25,   1.25,   0.75,   1.5,    1,      1.5 }
        };
        PTM = new double[,]
        {
            {1.25,  1.25,   1.5,    1.5,    1.5,    1 },
            {1,     1,      1.25,   1,      1.25,   1 },
            {1,     1,      1.25,   1,      1.25,   1 },
            {0.5,   1,      0.25,   0.75,   0.5,    1.5 },
            {0.75,  1,      0.5,    1.5,    1,      1.5 },
            {1,     1,      1,      1,      1,      1},
            {1,     1,      1,      1,      1,      1},
            {1,     1,      1,      1,      1,      1}
        };
        DTM = new double[,]
        {
            // Pesada	Ligera	Caballería	Arquero	Mago	Guerrillero	Curandero
            {0.75,  0.75,   1.25,   0.75,   0.75,   0.75,   0.75 },
            {0.75,  0.75,   1.25,   0.75,   0.75,   0.75,   0.75 },
            {1,     1,      1,      1,      1,      1,      1 },
            {1.5,   1.75,   1,      1.5,    1.75,   1.5,    1.5 },
            {1.25,  1.5,    1,      1.25,   1.5,    1.25,   1.25 },
            {1,     1,      1,      1,      1,      1,      1},
            {1,     1,      1,      1,      1,      1,      1},
            {1,     1,      1,      1,      1,      1,      1}
        };
        MTM = new double[,]
        {
            // Pesada	Ligera	Caballería	Arquero	Mago	Guerrillero	Curandero
            {1.25,  1.25,   1.5,    1.25,   1.25,   1.25, 1},   // Road
            {1,     1,      1.25,   1,      1,      1,    1},   // Plain
            {1,     1,      1.25,   1,      1,      1,    1},   // Desert
            {0.5,   1.25,   0.25,   0.75,   0.75,   1.5,  1},   // Forest
            {0.75,  1.5,    0.5,    1.5,    1,      1.5,  1},   // Mountain
            {1,     1,      1,      1,      1,      1,    1},   // RedBase
            {1,     1,      1,      1,      1,      1,    1},   // BlueBase
            {1,     1,      1,      1,      1,      1,    1}    // River
        };
    }

    public static Modifier GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Modifier();
        }
        return _instance;
    }

    public double getPrecisionModifier(Unit Attacker, Unit Defender)
    {
        return PTM[(int)Attacker.GetTerrainUnderUnit(), (int)Attacker.getType()] / DTM[(int)Defender.GetTerrainUnderUnit(), (int)Defender.getType()];
    }

    public double getPrecisionModifier(Unit.Type attackerType, Unit.Type defenderType, TipoTerreno attackerTerrain, TipoTerreno defenderTerrain)
    {
        return PTM[(int)attackerTerrain, (int)attackerType] / DTM[(int)defenderTerrain, (int)defenderType];
    }

    public double getAttackModifier(Unit Attacker, Unit Defender)
    {
        return AM[(int)Attacker.getType(), (int)Defender.getType()];
    }

    public double getAttackModifier(Unit.Type attackerType, Unit.Type defenderType)
    {
        return AM[(int)attackerType, (int)defenderType];
    }

    public double getMovementModifier(Unit unit, TerrenoInfo terrain)
    {
        return getMovementModifier(unit.getType(), terrain.TerrainType);
    }

    public double getMovementModifier(Unit.Type unitType, TipoTerreno terrain)
    {
        return MTM[(int)terrain, (int)unitType];
    }
}
