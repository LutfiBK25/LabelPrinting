namespace LabelPrinting.Domain.Entities.Label.Elements
{
    public class LabelPictureElement : LabelElement
    {
        public string ImageBase64 { get; set; }

        public static LabelPictureElement FromFile(string path)
        {
            return new LabelPictureElement
            {
                ImageBase64 = Convert.ToBase64String(File.ReadAllBytes(path))
            };
        }

        public void SaveToFile(string path)
        {
            File.WriteAllBytes(path, Convert.FromBase64String(ImageBase64));
        }
    }
}
