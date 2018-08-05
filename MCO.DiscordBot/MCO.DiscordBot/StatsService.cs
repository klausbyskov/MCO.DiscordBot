using Binance.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCO.DiscordBot
{
    public class StatsService
    {
        public StatsService()
        {
            var coll = new PrivateFontCollection();
            coll.AddFontFile("Roboto-Regular.ttf");
            coll.AddFontFile("Roboto-Bold.ttf");
            coll.AddFontFile("Roboto-Medium.ttf");
        }

        public async Task<byte[]> GetStatsImageAsync()
        {
            var binanceClient = new BinanceClient();
            var btcMcoPrice = await binanceClient.Get24HPriceAsync("MCOBTC");
            var ethMcoPrice = await binanceClient.Get24HPriceAsync("MCOETH");
            var btcUsdtPrice = await binanceClient.Get24HPriceAsync("BTCUSDT");

            var mcoDollarPrice = btcMcoPrice.Data.LastPrice * btcUsdtPrice.Data.LastPrice;
            var mcoDollarPercentage = ((1 + btcMcoPrice.Data.PriceChangePercent / 100) * (1 + btcUsdtPrice.Data.PriceChangePercent / 100) - 1) * 100m;

            var greenColor = Color.FromArgb(112, 168, 0);
            var redColor = Color.FromArgb(234, 0, 112);
            var mcoBlueColor = Color.FromArgb(0, 45, 114);
            var mcoGrayColor = Color.FromArgb(33, 37, 41);
            var lightGrayColor = Color.FromArgb(128, 128, 128);

            var largeFont = new Font("Roboto Medium", 28);
            var mediumFont = new Font("Roboto Medium", 12);
            var smallFont = new Font("Roboto", 8);

            var mcoBluebrush = new SolidBrush(mcoBlueColor);
            var blackBrush = new SolidBrush(mcoGrayColor);
            var redBrush = new SolidBrush(redColor);
            var greenBrush = new SolidBrush(greenColor);
            var grayBrush = new SolidBrush(lightGrayColor);

            var reservations = await McoLifeService.GetReservations();

            var left = 10;
            var leftFix = 1;
            var top = 67;

            using(var bg = Bitmap.FromFile("MCO_bg.png"))
            {
                using(var memStr = new MemoryStream())
                {
                    var gr = Graphics.FromImage(bg);
                    gr.TextRenderingHint = TextRenderingHint.AntiAlias;

                    gr.DrawString(FormattableString.Invariant($"MCO Stats on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"), smallFont, grayBrush, 186, 11);

                    var usdPercentage = FormattableString.Invariant($"{mcoDollarPercentage:0.00}%");
                    var usdPercentageSize = gr.MeasureString(usdPercentage, mediumFont);
                    gr.DrawString(usdPercentage, mediumFont, mcoDollarPercentage >= 0m ? greenBrush : redBrush, bg.Width - usdPercentageSize.Width - left, top + 35);
                    var usdPrice = FormattableString.Invariant($"${mcoDollarPrice:0.00}");
                    var usdPriceSize = gr.MeasureString(usdPrice, largeFont);
                    gr.DrawString(usdPrice, largeFont, mcoBluebrush, bg.Width - usdPriceSize.Width - left - usdPercentageSize.Width + 8, top + 16);

                    gr.DrawString("Price", smallFont, grayBrush, left + leftFix, top += 10);

                    var btcPrice = FormattableString.Invariant($"{btcMcoPrice.Data.LastPrice:0.000000} BTC");
                    var btcPriceSize = gr.MeasureString(btcPrice, mediumFont);

                    gr.DrawString(btcPrice, mediumFont, blackBrush, left, top += 10);
                    gr.DrawString(FormattableString.Invariant($"{btcMcoPrice.Data.PriceChangePercent:0.00}%"), mediumFont, btcMcoPrice.Data.PriceChangePercent >= 0 ? greenBrush : redBrush, left + btcPriceSize.Width, top);

                    var ethPrice = FormattableString.Invariant($"{ethMcoPrice.Data.LastPrice:0.000000} ETH");
                    var ethPriceSize = gr.MeasureString(ethPrice, mediumFont);

                    gr.DrawString(ethPrice, mediumFont, blackBrush, left, top += 15);
                    gr.DrawString(FormattableString.Invariant($"{ethMcoPrice.Data.PriceChangePercent:0.00}%"), mediumFont, ethMcoPrice.Data.PriceChangePercent >= 0 ? greenBrush : redBrush, left + ethPriceSize.Width, top);

                    gr.DrawString("24H Volume (Binance)", smallFont, grayBrush, left + leftFix, top += 20);
                    gr.DrawString(FormattableString.Invariant($"{btcMcoPrice.Data.QuoteVolume:0.00} BTC / {btcMcoPrice.Data.Volume:#,##0.00} MCO"), mediumFont, blackBrush, left, top += 11);
                    gr.DrawString(FormattableString.Invariant($"{ethMcoPrice.Data.QuoteVolume:0.00} ETH / {ethMcoPrice.Data.Volume:#,##0.00} MCO"), mediumFont, blackBrush, left, top += 15);

                    gr.DrawString("Reservations", smallFont, grayBrush, left + leftFix, top += 20);
                    gr.DrawString(FormattableString.Invariant($"Total: {reservations.Total:#,###}"), mediumFont, blackBrush, left, top += 10);
                    gr.DrawString(FormattableString.Invariant($"Today: {reservations.Today:#,##0}"), mediumFont, blackBrush, left, top += 15);

                    gr.DrawString("BTC Price", smallFont, grayBrush, left + leftFix, top += 20);
                    gr.DrawString(FormattableString.Invariant($"${btcUsdtPrice.Data.LastPrice:#,###,###.00}"), mediumFont, blackBrush, left, top += 10);

                    gr.Save();

                    bg.Save(memStr, ImageFormat.Png);

                    return memStr.ToArray();
                }
            }
        }
    }
}
