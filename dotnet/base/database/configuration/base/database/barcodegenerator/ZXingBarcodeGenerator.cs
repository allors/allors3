// <copyright file="BarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.IO;
    using Domain;
    using SkiaSharp;
    using ZXing;
    using ZXing.Common;
    using ZXing.SkiaSharp;

    public class ZXingBarcodeGenerator : IBarcodeGenerator
    {
        public byte[] Generate(string content, BarcodeType type, int? width, int? height, int? margin, bool? pure)
        {
            BarcodeFormat barcodeFormat;
            switch (type)
            {
                case BarcodeType.AZTEC:
                    barcodeFormat = BarcodeFormat.AZTEC;
                    break;
                case BarcodeType.CODABAR:
                    barcodeFormat = BarcodeFormat.CODABAR;
                    break;
                case BarcodeType.CODE_39:
                    barcodeFormat = BarcodeFormat.CODE_39;
                    break;
                case BarcodeType.CODE_93:
                    barcodeFormat = BarcodeFormat.CODE_93;
                    break;
                case BarcodeType.CODE_128:
                    barcodeFormat = BarcodeFormat.CODE_128;
                    break;
                case BarcodeType.DATA_MATRIX:
                    barcodeFormat = BarcodeFormat.DATA_MATRIX;
                    break;
                case BarcodeType.EAN_8:
                    barcodeFormat = BarcodeFormat.EAN_8;
                    break;
                case BarcodeType.EAN_13:
                    barcodeFormat = BarcodeFormat.EAN_13;
                    break;
                case BarcodeType.ITF:
                    barcodeFormat = BarcodeFormat.ITF;
                    break;
                case BarcodeType.MAXICODE:
                    barcodeFormat = BarcodeFormat.MAXICODE;
                    break;
                case BarcodeType.PDF_417:
                    barcodeFormat = BarcodeFormat.PDF_417;
                    break;
                case BarcodeType.QR_CODE:
                    barcodeFormat = BarcodeFormat.QR_CODE;
                    break;
                case BarcodeType.RSS_14:
                    barcodeFormat = BarcodeFormat.RSS_14;
                    break;
                case BarcodeType.RSS_EXPANDED:
                    barcodeFormat = BarcodeFormat.RSS_EXPANDED;
                    break;
                case BarcodeType.UPC_A:
                    barcodeFormat = BarcodeFormat.UPC_A;
                    break;
                case BarcodeType.UPC_E:
                    barcodeFormat = BarcodeFormat.UPC_E;
                    break;
                case BarcodeType.UPC_EAN_EXTENSION:
                    barcodeFormat = BarcodeFormat.UPC_EAN_EXTENSION;
                    break;
                case BarcodeType.MSI:
                    barcodeFormat = BarcodeFormat.MSI;
                    break;
                case BarcodeType.PLESSEY:
                    barcodeFormat = BarcodeFormat.PLESSEY;
                    break;
                case BarcodeType.IMB:
                    barcodeFormat = BarcodeFormat.IMB;
                    break;
                default:
                    throw new ArgumentException();
            }

            var barcodeWriter = new BarcodeWriter
            {
                Format = barcodeFormat,
            };

            if (width.HasValue || height.HasValue || margin.HasValue)
            {
                barcodeWriter.Options = new EncodingOptions();
                if (width.HasValue)
                {
                    barcodeWriter.Options.Width = width.Value;
                }

                if (height.HasValue)
                {
                    barcodeWriter.Options.Height = height.Value;
                }

                if (margin.HasValue)
                {
                    barcodeWriter.Options.Margin = margin.Value;
                }

                if (pure.HasValue)
                {
                    barcodeWriter.Options.PureBarcode = pure.Value;
                }
            }

            using (var image = barcodeWriter.Write(content))
            {
                using (var memStream = new MemoryStream())
                {
                    using (var wstream = new SKManagedWStream(memStream))
                    {
                        if (image.Encode(wstream, SKEncodedImageFormat.Png, 100))
                        {
                            return memStream.ToArray();
                        }

                        return null;
                    }
                }
            }
        }
    }
}
