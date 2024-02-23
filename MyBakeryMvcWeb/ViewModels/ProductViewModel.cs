namespace MyBakeryMvcWeb.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ProductViewModel : IValidatableObject
    {
        [Display(Name = "產品代號")]
        [Required(ErrorMessage = "{0} 必須要有值")]
        public int Id { get; set; }

        [Display(Name = "產品名稱")]
        [Required(ErrorMessage = "{0} 必須要有值")]
        [StringLength(10, ErrorMessage = "產品名稱長度最多為 10 個字元")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "訂購數量")]
        [Range(1, 10, ErrorMessage = "{0} 的資料值, 必須介於 {1} ~ {2}.")]
        public int OrderQty { get; set; }

        [Display(Name = "產品單價")]
        public int UnitPrice { get; set; }


        /// <summary>
        /// Validates the specified validation context.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <remarks>
        /// 重要: 只有在前述的 Validation Attribute 都通過以後, 才會執行這裡的檢核.
        /// 亦即: (1) Validation Attribute 有誤, 前端只會看到 Validation Attribute 的錯誤.
        ///       (2) Validation Attribute 正確, 前端才看到以下的錯誤.   
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Custom validation logic for UnitPrice
            if (UnitPrice < 1 || UnitPrice > 1000)
            {
                string[] memberNames = [nameof(UnitPrice)];
                yield return new ValidationResult("產品單價必須在 1 ~ 1000", memberNames);
            }
        }
    }
}
