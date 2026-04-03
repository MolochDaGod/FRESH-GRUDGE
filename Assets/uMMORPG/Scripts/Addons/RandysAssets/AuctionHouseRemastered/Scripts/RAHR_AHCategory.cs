[System.Serializable]
public class RAHR_AHCategory {
    public string category;
    public bool isSubCategory = false;

    public RAHR_AHCategory() { }

    public RAHR_AHCategory(string category, bool isSubCategory = false) {
        this.category = category;
        this.isSubCategory = isSubCategory;
    }
}