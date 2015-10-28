using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AutoCountDemo
{
    class sqlHelp
    {
        /// <summary>
        /// 获取随工单信息
        /// </summary>
        public const string GetLotsnINfo = @"DECLARE	@return_value int,        
                                             @I_ReturnMessage nvarchar(max)
                                             EXEC @return_value = [dbo].[Txn_DD_GetLotsnInfo]
                                                    @I_ReturnMessage = @I_ReturnMessage OUTPUT,
                                                    @LotSN = N'{0}'                                                   
                                             SELECT @I_ReturnMessage as N'@I_ReturnMessage','Return Value' = @return_value";
       

        /// 获取工单
        /// </summary>
        public const string GetMO = @"DECLARE	@return_value int,
									@I_ReturnMessage nvarchar(max)										
							EXEC	@return_value = [dbo].[TXN_GetDDMOInfo]
									@I_ReturnMessage = @I_ReturnMessage OUTPUT,
									@Lotsn = N'{0}' 
                        SELECT	@I_ReturnMessage as N'@I_ReturnMessage','Return Value' = @return_value";



        /// <summary>
        /// 获取工单的详细信息
        /// </summary>
        public const string GeMOinfo = @"select --ms.ScheduleNo,
		                 mo.id as '工单ID号',mo.MOName,pr.ProductName ,p.ProductDescription+p.ProductDescription1 as ProductDescription,
		                PlannedDateFrom,MOQtyRequired,isnull(mo.RealmaterialQty,0) as Realmaterialpcs,DDmaterialQty,pg.BoxNum as BZWeight ,
						'' as JGSingleWeight
                        --,p.FXSingleBoxNum as '单盒数量',p.UpTolerance,p.LowTolerance
	                    from mo 
	                    join Product p on p.ProductId=mo.ProductId
	                    join ProductRoot pr on pr.ProductRootId=p.ProductRootId
	                    left JOIN dbo.packag pg ON pg.productNO=pr.ProductName
						--join MOSchedule ms on ms.MOId=mo.MOId
						where mo.ID='{0}'";

        /// <summary>
        /// 修改单重
        /// </summary>
        public const string SetpieceWeight = @" UPDATE  Product
                                                SET JGSingleWeight = {0}
                                                FROM dbo.Product p
                                                        JOIN ProductRoot pr ON p.ProductRootId = pr.ProductRootId
                                                        WHERE pr.ProductName= '{1}'";

        /// <summary>
        /// 打印条码
        /// </summary>
        public const string PrintBSD = @"DECLARE	@return_value int,
										@I_ReturnMessage nvarchar(max)
								EXEC	@return_value = [dbo].[TXN_PrintBZBQ_1]
										@I_ReturnMessage = @I_ReturnMessage OUTPUT,
										@Lotsn = N'{0}',
                                        @BZWeight ={1} ,
                                        @pcs = {2},	
                                        @pieceWeight ={3},
                                        @moid = N'{4}',
                                        @LtWeight = {5},
                                        @LabelNum = {6},
                                        @totalNumber={7}, 
                                        @I_OrBitUserId='{8}'
                                        SELECT	@I_ReturnMessage as N'@I_ReturnMessage','Return Value' = @return_value";

        /// <summary>
        /// 获取Ftp服务器信息
        /// </summary>
        public const string FtpInfo = @"select ParameterValueEx from SysParameter where ParameterValue  ='LBD' and ParameterName='[Ftp Server Setting]'";

        /// <summary>
        /// 获取ftp信息
        /// </summary>
        public const string ftpname = "select FtpFileId+FtpFileName,FtpDirectory from FtpFile where FtpFileName='DDKHTM.mrz' order by CreateDate desc";

        /// <summary>
        /// 获取文件信息
        /// </summary>
        public const string GetFtpFile = " select FtpFileId+FtpFileName,FtpDirectory from FtpFile where FtpFileName='DDKHTM.mrz' order by CreateDate desc ";

        /// <summary>
        /// 保存称重信息
        /// </summary>
        public const string InsertLotSnWeigh = @"DECLARE	@return_value int,
		                                                    @I_ReturnMessage nvarchar(max)	

                                                    EXEC	@return_value = [dbo].[Txn_InsertLotSNWeigh]
		                                                    @I_ReturnMessage = @I_ReturnMessage OUTPUT,
		                                                    @Lotsn = N'{0}',
		                                                    @pcs = {1},
                                                            @isfg={2},
		                                                    @Weight = {3}
                                                    SELECT	@I_ReturnMessage as N'@I_ReturnMessage','Return Value' = @return_value";

        /// <summary>
        /// 获取零头数量
        /// </summary>
        public const string GetLTPCS = " SELECT PCS FROM DDLotSNWeighTable WHERE LOTSN = '{0}'";

        /// <summary>
        /// 获取配置信息
        /// </summary>
        public const string GetConfig = @"SELECT
                                                CountModuleIP,
                                                StandardPCSUpperLimit,
                                                PortName,
                                                LotSNAmount
                                            FROM DDPackconfigTable
                                            WHERE PackNO = '{0}'";

        public string a = "";
    }
}
