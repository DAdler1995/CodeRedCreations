using CodeRedCreations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Methods
{
    public static class Static
    {
        public static async Task<string> GetImageSrcAsync(byte[] bytes)
        {
            string imgSrc = string.Empty;
            await Task.Run(() =>
            {
                var base64 = Convert.ToBase64String(bytes);
                imgSrc = $"data:image/gif;base64, {base64}";
            });

            return imgSrc;
        }

        public static async Task<string> GetImageSrcAsync(ProductModel product)
        {
            string imgSrc = "/images/Photo-Not-Available.png";
            if (product.Images.Count > 0)
            {
                await Task.Run(() =>
                {
                    var base64 = Convert.ToBase64String(product.Images.FirstOrDefault().Bytes);
                    imgSrc = $"data:image/gif;base64, {base64}";
                });
            }

            return imgSrc;
        }
    }
}
