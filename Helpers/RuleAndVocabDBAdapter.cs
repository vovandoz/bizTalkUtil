using System.Collections;
using Microsoft.RuleEngine;
using Configuration = Microsoft.RuleEngine.Configuration;

namespace Breutil.Helpers
{
    public class  RuleAndVocabDbAdapter
    {
        /// <summary>
        /// подключается к БД и возвращает все rule из БД
        /// </summary>
        /// <param name="severName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static RuleSetInfo[] GetRuleSetInfo(string severName, string dbName)
        {
            RuleStore ruleStore = ((IRuleSetDeploymentDriver)Microsoft.RuleEngine.RemoteUpdateService.RemoteUpdateService.LocateObject(Configuration.DeploymentDriverClass, Configuration.DeploymentDriverDll, new ArrayList()
            {
                severName,
                dbName
            }.ToArray())).GetRuleStore();
            var ruleSetInfoCollection = ruleStore.GetRuleSets(RuleStore.Filter.All);


            var ruleSet = new RuleSetInfo[ruleSetInfoCollection.Count];
            ruleSetInfoCollection.CopyTo(ruleSet, 0);
            return ruleSet;
        }
        /// <summary>
        /// подключается к БД и возвращает все vocab(включая default vocab) из БД
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static VocabularyInfo[] GetVocabularyInfoSet(string serverName, string dbName)
        {
            RuleStore vocabStore = ((IRuleSetDeploymentDriver)Microsoft.RuleEngine.RemoteUpdateService.RemoteUpdateService.LocateObject(Configuration.DeploymentDriverClass, Configuration.DeploymentDriverDll, new ArrayList()
            {
                serverName,
                dbName
            }.ToArray())).GetRuleStore();
            var vocabularyInfoCollection = vocabStore.GetVocabularies(RuleStore.Filter.All);


            var vocabularySet = new VocabularyInfo[vocabularyInfoCollection.Count];
            vocabularyInfoCollection.CopyTo(vocabularySet, 0);
            return vocabularySet;
        }
        /// <summary>
        /// возвращает объект driver, используемый для дальнейшей записи\чтения rule/vocab в\из БД
        /// </summary>
        /// <param name="severName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static IRuleSetDeploymentDriver GetDeploymentDriver(string severName, string dbName)
        {
            var deploymentDriver = (IRuleSetDeploymentDriver)Microsoft.RuleEngine.RemoteUpdateService.RemoteUpdateService.LocateObject(Configuration.DeploymentDriverClass, Configuration.DeploymentDriverDll, new ArrayList()
            {
                severName,
                dbName
            }.ToArray());

            return deploymentDriver;
        }

    }
}
