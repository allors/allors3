// <copyright file="UnitsOfMeasure.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class UnitsOfMeasure
    {
        // Quantity
        internal static readonly Guid PackId = new Guid("C4EC577A-D682-433c-BD70-84538BE83209");
        internal static readonly Guid PairId = new Guid("62CB31EB-CD70-4836-B20F-1088C6CA9DCB");
        internal static readonly Guid PieceId = new Guid("F4BBDB52-3441-4768-92D4-729C6C5D6F1B");
        internal static readonly Guid PeopleId = new Guid("13274801-52FD-47E8-A15B-8508E848C140");

        // Length, width, distance, thickness
        internal static readonly Guid MillimeterId = new Guid("A7F83AEF-20DB-42D8-9AB6-2D5821353BE8");
        internal static readonly Guid CentimeterId = new Guid("7D81FFC7-E77D-4a00-916D-49F2B1CCA12E");
        internal static readonly Guid MeterId = new Guid("2598BA8D-CF49-47f5-98E2-E65795C4178E");
        internal static readonly Guid KilometerId = new Guid("2598BA8D-CF49-47f5-98E2-E65795C4178E");

        // Mass
        internal static readonly Guid MilligramId = new Guid("43E19BE3-5F7D-441D-A6B3-52EC1B7A2F84");
        internal static readonly Guid GramId = new Guid("F108E442-3A85-4F69-81ED-51BA9FC39A39");
        internal static readonly Guid KilogramId = new Guid("652A99BB-2B11-4DA7-B938-7A8EC1061A09");
        internal static readonly Guid MetricTonId = new Guid("140145F7-EB1C-45B9-9B94-982254B88B5D");

        // Area
        internal static readonly Guid SquareMeterId = new Guid("B2BEEECC-4C04-487A-BA79-EE48AC555800");
        internal static readonly Guid HectareId = new Guid("7B28A3B1-0717-47E1-9AD9-133D885B7F7A");
        internal static readonly Guid SquareKilometerId = new Guid("4FA765C6-FAF0-4C72-808D-71E25603A45D");

        // Volume
        internal static readonly Guid MilliLiterId = new Guid("6DE02A90-1605-46B4-9BD5-88BD4690A5B2");
        internal static readonly Guid CubicCentimeterId = new Guid("F6289FF0-87E8-4ADC-92E3-C5D532BA515F");
        internal static readonly Guid LiterId = new Guid("C0F2845D-3CBC-4FE7-970B-8C06AFB75ABC");
        internal static readonly Guid CubicMeterId = new Guid("F573E22F-9450-4E0A-B177-4142DFAAE829");

        // Velocity
        internal static readonly Guid MeterPerSecondId = new Guid("66A4216C-46E8-4212-B456-76F968505F25");
        internal static readonly Guid KilometerPerHourId = new Guid("E3A61B56-2CD1-42E6-9360-1F83F3D15195");

        // Density
        internal static readonly Guid KilogramPerCubicMeterId = new Guid("C4519054-1F73-4C81-92C8-DFF2FBB12F4A");

        // Force
        internal static readonly Guid NewtonId = new Guid("E569CBD7-9975-4A19-9C42-BE8219FB954D");

        // Pressure
        internal static readonly Guid KiloPascalId = new Guid("65F45C76-FDA6-4A79-9A46-A676148B0E45");

        // Power
        internal static readonly Guid WattId = new Guid("ADF494B5-120F-475E-95F0-3EFC2B64FBE7");
        internal static readonly Guid KiloWattId = new Guid("71B7DCDD-A98D-4005-AFE8-8C5366847ECB");

        // Energy
        internal static readonly Guid KiloJouleId = new Guid("9F31AAF8-B4BC-48A3-A34B-795E4B082CEC");
        internal static readonly Guid MegaJouleId = new Guid("56D154BA-F16B-43AC-BCD7-B81EC670932D");
        internal static readonly Guid KiloWattHourId = new Guid("83C65E69-0048-4474-9F53-4EA89FA26194");

        // Temperature
        internal static readonly Guid DegreeCelsiusId = new Guid("A9D40912-E1DE-4A75-8CEE-8AE7FD4E9F3D");

        // Electric
        internal static readonly Guid AmpereId = new Guid("6CAA7DD3-608F-40A6-AE26-9141517D8C45");
        internal static readonly Guid VoltId = new Guid("A15B7AFB-660C-455D-A5C7-03D3D32B29CB");

        private UniquelyIdentifiableCache<UnitOfMeasure> cache;

        public UnitOfMeasure Pack => this.Cache[PackId];

        public UnitOfMeasure Pair => this.Cache[PairId];

        public UnitOfMeasure Piece => this.Cache[PieceId];

        public UnitOfMeasure People => this.Cache[PeopleId];

        public UnitOfMeasure Millimeter => this.Cache[MillimeterId];

        public UnitOfMeasure Centimeter => this.Cache[CentimeterId];

        public UnitOfMeasure Meter => this.Cache[MeterId];

        public UnitOfMeasure Kilometer => this.Cache[KilometerId];

        public UnitOfMeasure Milligram => this.Cache[MilligramId];

        public UnitOfMeasure Gram => this.Cache[GramId];

        public UnitOfMeasure Kilogram => this.Cache[KilogramId];

        public UnitOfMeasure MetricTon => this.Cache[MetricTonId];

        public UnitOfMeasure SquareMeter => this.Cache[SquareMeterId];

        public UnitOfMeasure Hectare => this.Cache[HectareId];

        public UnitOfMeasure SquareKilometer => this.Cache[SquareKilometerId];

        public UnitOfMeasure MilliLiter => this.Cache[MilliLiterId];

        public UnitOfMeasure CubicCentimeter => this.Cache[CubicCentimeterId];

        public UnitOfMeasure Liter => this.Cache[LiterId];

        public UnitOfMeasure CubicMeter => this.Cache[CubicMeterId];

        public UnitOfMeasure MeterPerSecond => this.Cache[MeterPerSecondId];

        public UnitOfMeasure KilometerPerHour => this.Cache[KilometerPerHourId];

        public UnitOfMeasure KilogramPerCubicMeter => this.Cache[KilogramPerCubicMeterId];

        public UnitOfMeasure Newton => this.Cache[NewtonId];

        public UnitOfMeasure KiloPascal => this.Cache[KiloPascalId];

        public UnitOfMeasure Watt => this.Cache[WattId];

        public UnitOfMeasure KiloWatt => this.Cache[KiloWattId];

        public UnitOfMeasure KiloJoule => this.Cache[KiloJouleId];

        public UnitOfMeasure MegaJoule => this.Cache[MegaJouleId];

        public UnitOfMeasure KiloWattHour => this.Cache[KiloWattHourId];

        public UnitOfMeasure DegreeCelsius => this.Cache[DegreeCelsiusId];

        public UnitOfMeasure Ampere => this.Cache[AmpereId];

        public UnitOfMeasure Volt => this.Cache[VoltId];

        private UniquelyIdentifiableCache<UnitOfMeasure> Cache => this.cache ??= new UniquelyIdentifiableCache<UnitOfMeasure>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            var spanishLocale = new Locales(this.Transaction).Spanish;

            if (this.Pack == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("pack")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("pakket").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("paquete").WithLocale(spanishLocale).Build())
                    .WithUniqueId(PackId)
                    .WithAbbreviation("pack")
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Pair == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("pair")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("paar").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("par").WithLocale(spanishLocale).Build())
                    .WithUniqueId(PairId)
                    .WithAbbreviation("pair")
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Piece == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("piece")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("stuk").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("cada").WithLocale(spanishLocale).Build())
                    .WithUniqueId(PieceId)
                    .WithIsActive(true)
                    .WithAbbreviation("piece")
                    .Build();
            }

            if (this.People == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("people")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("personen").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("personas").WithLocale(spanishLocale).Build())
                    .WithUniqueId(PeopleId)
                    .WithAbbreviation("people")
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Millimeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("millimeter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("millimeter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("milímetro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("mm")
                    .WithUniqueId(MillimeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Centimeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("centimeter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("centimeter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("centímetro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("cm")
                    .WithUniqueId(CentimeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Meter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("meter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("meter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("metro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("m")
                    .WithUniqueId(MeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Kilometer == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilometer")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilometer").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilometro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("km")
                    .WithUniqueId(KilometerId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Milligram == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("milligram")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("milligram").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("miligramo").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("mg")
                    .WithUniqueId(MilligramId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Gram == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("gram")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("gram").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("gramo").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("g")
                    .WithUniqueId(GramId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Kilogram == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilogram")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilogram").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilogramo").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kg")
                    .WithUniqueId(KilogramId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.MetricTon == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("metric ton")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("ton").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("tonelada métrica").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("t")
                    .WithUniqueId(MetricTonId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.DegreeCelsius == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("degree Celsius")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("graden Celsius").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("grado Celsius").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("°C")
                    .WithUniqueId(DegreeCelsiusId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.SquareMeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("square meter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("vierkante meter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("metro cuadrado").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("m²")
                    .WithUniqueId(SquareMeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Hectare == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("hectare")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("hectare").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("hectárea").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("ha")
                    .WithUniqueId(HectareId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.SquareKilometer == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("square kilometer")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("vierkante kilometer").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilometro cuadrado").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("km²")
                    .WithUniqueId(SquareKilometerId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.MilliLiter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("milliliter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("milliliter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("mililitro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("ml")
                    .WithUniqueId(MilliLiterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.CubicCentimeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("Cubic centimeter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Kubieke centimeter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Centímetro cúbico").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("cm³")
                    .WithUniqueId(CubicCentimeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Liter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("liter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("liter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("litro").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("L")
                    .WithUniqueId(LiterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.CubicMeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("Cubic meter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Kubieke meter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Metro cúbico").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("m³")
                    .WithUniqueId(CubicMeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.MeterPerSecond == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("meter per second")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("meter per seconde").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("metro por segundo").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("m/s")
                    .WithUniqueId(MeterPerSecondId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KilometerPerHour == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilometer per hour")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilometer per uur").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilómetro por hora").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("km/h")
                    .WithUniqueId(KilometerPerHourId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KilogramPerCubicMeter == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilogram per cubic meter")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilogram per kubieke meter").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilogramo por metro cúbico").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kg/m³")
                    .WithUniqueId(KilogramPerCubicMeterId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Newton == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("newton")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Newton").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Newton").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("N")
                    .WithUniqueId(NewtonId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloPascal == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilopascal")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilopascal").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilopascal").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kPa")
                    .WithUniqueId(KiloPascalId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.Watt == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("watt")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("watt").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("vatio").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("W")
                    .WithUniqueId(WattId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloWatt == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilowatt")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilowatt").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilovatio").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kW")
                    .WithUniqueId(KiloWattId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloJoule == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilojoule")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilojoule").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilojulio").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kJ")
                    .WithUniqueId(KiloJouleId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.MegaJoule == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("megajoule")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("megajoule").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("megajulio").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("MJ")
                    .WithUniqueId(MegaJouleId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloWattHour == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("kilowatt hour")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilowatt uur").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("kilovatio hora").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("kW·h")
                    .WithUniqueId(KiloWattHourId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloWattHour == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("ampere")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("ampere").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("amperio").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("A")
                    .WithUniqueId(KiloWattHourId)
                    .WithIsActive(true)
                    .Build();
            }

            if (this.KiloWattHour == null)
            {
                new UnitOfMeasureBuilder(this.Transaction)
                    .WithName("volt")
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("volt").WithLocale(dutchLocale).Build())
                    .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("voltios").WithLocale(spanishLocale).Build())
                    .WithAbbreviation("V")
                    .WithUniqueId(KiloWattHourId)
                    .WithIsActive(true)
                    .Build();
            }
        }
    }
}
