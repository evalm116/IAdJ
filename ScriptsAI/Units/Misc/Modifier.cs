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
        // AM [Atacante, Defensor]
        // Columnas/Filas: 0:Paladin, 1:Cleric, 2:Rogue, 3:Wizard, 4:Ranger
        AM = new double[,]
        {
            {1.0,  1.2,  1.4,  1.8,  1.6}, // Paladin
            {1.5,  1.0,  1.0,  1.1,  1.2}, // Cleric
            {0.8,  1.3,  1.0,  2.0,  1.5}, // Rogue
            {2.0,  0.9,  1.2,  1.0,  1.1}, // Wizard
            {0.7,  1.4,  1.2,  1.3,  1.0}  // Ranger
        };

        // PTM (FTA) [Terreno, Unidad]
        // Filas Terreno: 0:Road, 1:Plain, 2:Desert, 3:Forest, 4:Mountain...
        PTM = new double[,]
        {//Paladin, Cleric, Rogue, Wizard, Ranger
            {1.2,  1.0,  0.8,  1.0,  1.0}, // Road
            {1.1,  1.0,  1.0,  1.3,  1.2}, // Plain
            {1.0,  1.0,  1.0,  1.0,  1.0}, // Desert (Default)
            {0.4,  0.9,  1.8,  0.7,  1.5}, // Forest
            {0.8,  1.0,  1.2,  1.0,  1.2}, // Mountain
            {1.0,  1.0,  1.0,  1.0,  1.0}, // RedBase
            {1.0,  1.0,  1.0,  1.0,  1.0}, // BlueBase
            {1.0,  1.0,  1.0,  1.0,  1.0}  // River
        };

        // DTM (FTD) [Terreno, Unidad]
        DTM = new double[,]
        { //Paladin, Cleric, Rogue, Wizard, Ranger
            {0.8,  0.7,  0.6,  0.7,  0.5}, // Road
            {1.4,  1.0,  1.0,  1.0,  1.0}, // Plain
            {1.0,  1.0,  1.0,  1.0,  1.0}, // Desert
            {1.1,  1.0,  2.2,  1.3,  1.8}, // Forest
            {1.2,  1.1,  1.5,  1.0,  1.4}, // Mountain
            {1.5,  1.5,  1.5,  1.5,  1.5}, // RedBase (Defensivo)
            {1.5,  1.5,  1.5,  1.5,  1.5}, // BlueBase (Defensivo)
            {0.5,  0.5,  0.5,  0.5,  0.5}  // River (Vulnerable)
        };

        // MTM (Movimiento) [Terreno, Unidad]
        // Penalizaciones de velocidad por terreno
        MTM = new double[,]
        { //Paladin, Cleric, Rogue, Wizard, Ranger
            {1.25, 1.25, 1.25, 1.25, 1.25}, // Road (Mejorado)
            {1.0,  1.0,  1.0,  1.0,  1.0},  // Plain
            {0.8,  0.8,  0.8,  0.8,  0.8},  // Desert
            {0.5,  0.8,  1.5,  0.6,  1.2},  // Forest (Rogue y Ranger son rápidos aquí)
            {0.4,  0.6,  1.2,  0.5,  1.0},  // Mountain
            {1.0,  1.0,  1.0,  1.0,  1.0},  // RedBase
            {1.0,  1.0,  1.0,  1.0,  1.0},  // BlueBase
            {0.3,  0.3,  0.3,  0.3,  0.3}   // River (Infranqueable/Muy lento)
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
