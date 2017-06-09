namespace PZMMENYI.SQLiteServices {
    /// <summary>
    /// 数据文件路径。
    /// </summary>
    public interface IDatabaseFileHelper {
        /// <summary>
        /// 获得数据文件路径。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        string GetLocalDatabaseFilePath(string filename);
    }
}
