/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    ///     下载文件类型属性
    /// </summary>
    public sealed class TmphContentTypeInfo : Attribute
    {
        /// <summary>
        ///     扩展名关联下载文件类型
        /// </summary>
        private static readonly TmphStateSearcher.TmphAscii<byte[]> contentTypes;

        /// <summary>
        ///     未知扩展名关联下载文件类型
        /// </summary>
        private static readonly byte[] unknownContentType;

        /// <summary>
        ///     文件扩展名
        /// </summary>
        public string ExtensionName;

        /// <summary>
        ///     下载文件类型名称
        /// </summary>
        public string Name;

        static TmphContentTypeInfo()
        {
            var types = Enum.GetValues(typeof(TmphContentType))
                .toArray<TmphContentType>().getArray(value => TmphEnum<TmphContentType, TmphContentTypeInfo>.Array(value));
            contentTypes = new TmphStateSearcher.TmphAscii<byte[]>(types.getArray(value => value.ExtensionName),
                types.getArray(value => value.Name.GetBytes()));
            unknownContentType = contentTypes.Get("*");
        }

        /// <summary>
        ///     获取扩展名关联下载文件类型
        /// </summary>
        /// <param name="extensionName">扩展名</param>
        /// <returns>扩展名关联下载文件类型</returns>
        public static byte[] GetContentType(string extensionName)
        {
            return contentTypes.Get(extensionName, unknownContentType);
        }
    }

    /// <summary>
    ///     下载文件类型
    /// </summary>
    public enum TmphContentType
    {
        [TmphContentTypeInfo(ExtensionName = "*", Name = "application/octet-stream")]
        _,

        [TmphContentTypeInfo(ExtensionName = "323", Name = "text/h323")]
        _323,

        [TmphContentTypeInfo(ExtensionName = "907", Name = "drawing/907")]
        _907,

        [TmphContentTypeInfo(ExtensionName = "acp", Name = "audio/x-mei-aac")]
        acp,

        [TmphContentTypeInfo(ExtensionName = "ai", Name = "application/postscript")]
        ai,

        [TmphContentTypeInfo(ExtensionName = "aif", Name = "audio/aiff")]
        aif,

        [TmphContentTypeInfo(ExtensionName = "aifc", Name = "audio/aiff")]
        aifc,

        [TmphContentTypeInfo(ExtensionName = "aiff", Name = "audio/aiff")]
        aiff,

        [TmphContentTypeInfo(ExtensionName = "asa", Name = "text/asa")]
        asa,

        [TmphContentTypeInfo(ExtensionName = "asf", Name = "video/x-ms-asf")]
        asf,

        [TmphContentTypeInfo(ExtensionName = "asp", Name = "text/asp")]
        asp,

        [TmphContentTypeInfo(ExtensionName = "asx", Name = "video/x-ms-asf")]
        asx,

        [TmphContentTypeInfo(ExtensionName = "au", Name = "audio/basic")]
        au,

        [TmphContentTypeInfo(ExtensionName = "avi", Name = "video/avi")]
        avi,

        [TmphContentTypeInfo(ExtensionName = "awf", Name = "application/vnd.adobe.workflow")]
        awf,

        [TmphContentTypeInfo(ExtensionName = "biz", Name = "text/xml")]
        biz,

        [TmphContentTypeInfo(ExtensionName = "bmp", Name = "image/msbitmap")]
        bmp,

        [TmphContentTypeInfo(ExtensionName = "cat", Name = "application/vnd.ms-pki.seccat")]
        cat,

        [TmphContentTypeInfo(ExtensionName = "cdf", Name = "application/x-netcdf")]
        cdf,

        [TmphContentTypeInfo(ExtensionName = "cer", Name = "application/x-x509-ca-cert")]
        cer,

        [TmphContentTypeInfo(ExtensionName = "class", Name = "java/*")]
        _class,

        [TmphContentTypeInfo(ExtensionName = "cml", Name = "text/xml")]
        cml,

        [TmphContentTypeInfo(ExtensionName = "crl", Name = "application/pkix-crl")]
        crl,

        [TmphContentTypeInfo(ExtensionName = "crt", Name = "application/x-x509-ca-cert")]
        crt,

        [TmphContentTypeInfo(ExtensionName = "css", Name = "text/css")]
        css,

        [TmphContentTypeInfo(ExtensionName = "cur", Name = "image/x-icon")]
        cur,

        [TmphContentTypeInfo(ExtensionName = "dcd", Name = "text/xml")]
        dcd,

        [TmphContentTypeInfo(ExtensionName = "der", Name = "application/x-x509-ca-cert")]
        der,

        [TmphContentTypeInfo(ExtensionName = "dll", Name = "application/x-msdownload")]
        dll,

        [TmphContentTypeInfo(ExtensionName = "doc", Name = "application/msword")]
        doc,

        [TmphContentTypeInfo(ExtensionName = "dot", Name = "application/msword")]
        dot,

        [TmphContentTypeInfo(ExtensionName = "dtd", Name = "text/xml")]
        dtd,

        [TmphContentTypeInfo(ExtensionName = "edn", Name = "application/vnd.adobe.edn")]
        edn,

        [TmphContentTypeInfo(ExtensionName = "eml", Name = "message/rfc822")]
        eml,

        [TmphContentTypeInfo(ExtensionName = "ent", Name = "text/xml")]
        ent,

        [TmphContentTypeInfo(ExtensionName = "eps", Name = "application/postscript")]
        eps,

        [TmphContentTypeInfo(ExtensionName = "exe", Name = "application/x-msdownload")]
        exe,

        [TmphContentTypeInfo(ExtensionName = "fax", Name = "image/fax")]
        fax,

        [TmphContentTypeInfo(ExtensionName = "fdf", Name = "application/vnd.fdf")]
        fdf,

        [TmphContentTypeInfo(ExtensionName = "fif", Name = "application/fractals")]
        fif,

        [TmphContentTypeInfo(ExtensionName = "fo", Name = "text/xml")]
        fo,

        [TmphContentTypeInfo(ExtensionName = "gif", Name = "image/gif")]
        gif,

        [TmphContentTypeInfo(ExtensionName = "hpg", Name = "application/x-hpgl")]
        hpg,

        [TmphContentTypeInfo(ExtensionName = "hqx", Name = "application/mac-binhex40")]
        hqx,

        [TmphContentTypeInfo(ExtensionName = "hta", Name = "application/hta")]
        hta,

        [TmphContentTypeInfo(ExtensionName = "htc", Name = "text/x-component")]
        htc,

        [TmphContentTypeInfo(ExtensionName = "htm", Name = "text/html")]
        htm,

        [TmphContentTypeInfo(ExtensionName = "html", Name = "text/html")]
        html,

        [TmphContentTypeInfo(ExtensionName = "htt", Name = "text/webviewhtml")]
        htt,

        [TmphContentTypeInfo(ExtensionName = "htx", Name = "text/html")]
        htx,

        [TmphContentTypeInfo(ExtensionName = "ico", Name = "image/x-icon")]
        ico,

        [TmphContentTypeInfo(ExtensionName = "iii", Name = "application/x-iphone")]
        iii,

        [TmphContentTypeInfo(ExtensionName = "img", Name = "application/x-img")]
        img,

        [TmphContentTypeInfo(ExtensionName = "ins", Name = "application/x-internet-signup")]
        ins,

        [TmphContentTypeInfo(ExtensionName = "isp", Name = "application/x-internet-signup")]
        isp,

        [TmphContentTypeInfo(ExtensionName = "java", Name = "java/*")]
        java,

        [TmphContentTypeInfo(ExtensionName = "jfif", Name = "image/jpeg")]
        jfif,

        [TmphContentTypeInfo(ExtensionName = "jpe", Name = "image/jpeg")]
        jpe,

        [TmphContentTypeInfo(ExtensionName = "jpeg", Name = "image/jpeg")]
        jpeg,

        [TmphContentTypeInfo(ExtensionName = "jpg", Name = "image/jpeg")]
        jpg,

        [TmphContentTypeInfo(ExtensionName = "js", Name = "application/x-javascript")]
        js,

        [TmphContentTypeInfo(ExtensionName = "jsp", Name = "text/html")]
        jsp,

        [TmphContentTypeInfo(ExtensionName = "la1", Name = "audio/x-liquid-file")]
        la1,

        [TmphContentTypeInfo(ExtensionName = "lar", Name = "application/x-laplayer-reg")]
        lar,

        [TmphContentTypeInfo(ExtensionName = "latex", Name = "application/x-latex")]
        latex,

        [TmphContentTypeInfo(ExtensionName = "lavs", Name = "audio/x-liquid-secure")]
        lavs,

        [TmphContentTypeInfo(ExtensionName = "lmsff", Name = "audio/x-la-lms")]
        lmsff,

        [TmphContentTypeInfo(ExtensionName = "ls", Name = "application/x-javascript")]
        ls,

        [TmphContentTypeInfo(ExtensionName = "m1v", Name = "video/x-mpeg")]
        m1v,

        [TmphContentTypeInfo(ExtensionName = "m2v", Name = "video/x-mpeg")]
        m2v,

        [TmphContentTypeInfo(ExtensionName = "m3u", Name = "audio/mpegurl")]
        m3u,

        [TmphContentTypeInfo(ExtensionName = "m4e", Name = "video/mpeg4")]
        m4e,

        [TmphContentTypeInfo(ExtensionName = "man", Name = "application/x-troff-man")]
        man,

        [TmphContentTypeInfo(ExtensionName = "math", Name = "text/xml")]
        math,

        [TmphContentTypeInfo(ExtensionName = "mdb", Name = "application/msaccess")]
        mdb,

        [TmphContentTypeInfo(ExtensionName = "mfp", Name = "application/x-shockwave-flash")]
        mfp,

        [TmphContentTypeInfo(ExtensionName = "mht", Name = "message/rfc822")]
        mht,

        [TmphContentTypeInfo(ExtensionName = "mhtml", Name = "message/rfc822")]
        mhtml,

        [TmphContentTypeInfo(ExtensionName = "mid", Name = "audio/mid")]
        mid,

        [TmphContentTypeInfo(ExtensionName = "midi", Name = "audio/mid")]
        midi,

        [TmphContentTypeInfo(ExtensionName = "mml", Name = "text/xml")]
        mml,

        [TmphContentTypeInfo(ExtensionName = "mnd", Name = "audio/x-musicnet-download")]
        mnd,

        [TmphContentTypeInfo(ExtensionName = "mns", Name = "audio/x-musicnet-stream")]
        mns,

        [TmphContentTypeInfo(ExtensionName = "mocha", Name = "application/x-javascript")]
        mocha,

        [TmphContentTypeInfo(ExtensionName = "movie", Name = "video/x-sgi-movie")]
        movie,

        [TmphContentTypeInfo(ExtensionName = "mp1", Name = "audio/mp1")]
        mp1,

        [TmphContentTypeInfo(ExtensionName = "mp2", Name = "audio/mp2")]
        mp2,

        [TmphContentTypeInfo(ExtensionName = "mp2v", Name = "video/mpeg")]
        mp2v,

        [TmphContentTypeInfo(ExtensionName = "mp3", Name = "audio/mp3")]
        mp3,

        [TmphContentTypeInfo(ExtensionName = "mp4", Name = "video/mpeg4")]
        mp4,

        [TmphContentTypeInfo(ExtensionName = "mpa", Name = "video/x-mpg")]
        mpa,

        [TmphContentTypeInfo(ExtensionName = "mpd", Name = "application/vnd.ms-project")]
        mpd,

        [TmphContentTypeInfo(ExtensionName = "mpe", Name = "video/x-mpeg")]
        mpe,

        [TmphContentTypeInfo(ExtensionName = "mpeg", Name = "video/mpg")]
        mpeg,

        [TmphContentTypeInfo(ExtensionName = "mpg", Name = "video/mpg")]
        mpg,

        [TmphContentTypeInfo(ExtensionName = "mpga", Name = "audio/rn-mpeg")]
        mpga,

        [TmphContentTypeInfo(ExtensionName = "mpp", Name = "application/vnd.ms-project")]
        mpp,

        [TmphContentTypeInfo(ExtensionName = "mps", Name = "video/x-mpeg")]
        mps,

        [TmphContentTypeInfo(ExtensionName = "mpt", Name = "application/vnd.ms-project")]
        mpt,

        [TmphContentTypeInfo(ExtensionName = "mpv", Name = "video/mpg")]
        mpv,

        [TmphContentTypeInfo(ExtensionName = "mpv2", Name = "video/mpeg")]
        mpv2,

        [TmphContentTypeInfo(ExtensionName = "mpw", Name = "application/vnd.ms-project")]
        mpw,

        [TmphContentTypeInfo(ExtensionName = "mpx", Name = "application/vnd.ms-project")]
        mpx,

        [TmphContentTypeInfo(ExtensionName = "mtx", Name = "text/xml")]
        mtx,

        [TmphContentTypeInfo(ExtensionName = "mxp", Name = "application/x-mmxp")]
        mxp,

        [TmphContentTypeInfo(ExtensionName = "net", Name = "image/pnetvue")]
        net,

        [TmphContentTypeInfo(ExtensionName = "nws", Name = "message/rfc822")]
        nws,

        [TmphContentTypeInfo(ExtensionName = "odc", Name = "text/x-ms-odc")]
        odc,

        [TmphContentTypeInfo(ExtensionName = "p10", Name = "application/pkcs10")]
        p10,

        [TmphContentTypeInfo(ExtensionName = "p12", Name = "application/x-pkcs12")]
        p12,

        [TmphContentTypeInfo(ExtensionName = "p7b", Name = "application/x-pkcs7-certificates")]
        p7b,

        [TmphContentTypeInfo(ExtensionName = "p7c", Name = "application/pkcs7-mime")]
        p7c,

        [TmphContentTypeInfo(ExtensionName = "p7m", Name = "application/pkcs7-mime")]
        p7m,

        [TmphContentTypeInfo(ExtensionName = "p7r", Name = "application/x-pkcs7-certreqresp")]
        p7r,

        [TmphContentTypeInfo(ExtensionName = "p7s", Name = "application/pkcs7-signature")]
        p7s,

        [TmphContentTypeInfo(ExtensionName = "pcx", Name = "image/x-pcx")]
        pcx,

        [TmphContentTypeInfo(ExtensionName = "pdf", Name = "application/pdf")]
        pdf,

        [TmphContentTypeInfo(ExtensionName = "pdx", Name = "application/vnd.adobe.pdx")]
        pdx,

        [TmphContentTypeInfo(ExtensionName = "pfx", Name = "application/x-pkcs12")]
        pfx,

        [TmphContentTypeInfo(ExtensionName = "pic", Name = "application/x-pic")]
        pic,

        [TmphContentTypeInfo(ExtensionName = "pko", Name = "application/vnd.ms-pki.pko")]
        pko,

        [TmphContentTypeInfo(ExtensionName = "pl", Name = "application/x-perl")]
        pl,

        [TmphContentTypeInfo(ExtensionName = "plg", Name = "text/html")]
        plg,

        [TmphContentTypeInfo(ExtensionName = "pls", Name = "audio/scpls")]
        pls,

        [TmphContentTypeInfo(ExtensionName = "png", Name = "image/png")]
        png,

        [TmphContentTypeInfo(ExtensionName = "pot", Name = "application/vnd.ms-powerpoint")]
        pot,

        [TmphContentTypeInfo(ExtensionName = "ppa", Name = "application/vnd.ms-powerpoint")]
        ppa,

        [TmphContentTypeInfo(ExtensionName = "pps", Name = "application/vnd.ms-powerpoint")]
        pps,

        [TmphContentTypeInfo(ExtensionName = "ppt", Name = "application/vnd.ms-powerpoint")]
        ppt,

        [TmphContentTypeInfo(ExtensionName = "prf", Name = "application/pics-rules")]
        prf,

        [TmphContentTypeInfo(ExtensionName = "ps", Name = "application/postscript")]
        ps,

        [TmphContentTypeInfo(ExtensionName = "pwz", Name = "application/vnd.ms-powerpoint")]
        pwz,

        [TmphContentTypeInfo(ExtensionName = "r3t", Name = "text/vnd.rn-realtext3d")]
        r3t,

        [TmphContentTypeInfo(ExtensionName = "ra", Name = "audio/vnd.rn-realaudio")]
        ra,

        [TmphContentTypeInfo(ExtensionName = "ram", Name = "audio/x-pn-realaudio")]
        ram,

        [TmphContentTypeInfo(ExtensionName = "rat", Name = "application/rat-file")]
        rat,

        [TmphContentTypeInfo(ExtensionName = "rdf", Name = "text/xml")]
        rdf,

        [TmphContentTypeInfo(ExtensionName = "rec", Name = "application/vnd.rn-recording")]
        rec,

        [TmphContentTypeInfo(ExtensionName = "rjs", Name = "application/vnd.rn-realsystem-rjs")]
        rjs,

        [TmphContentTypeInfo(ExtensionName = "rjt", Name = "application/vnd.rn-realsystem-rjt")]
        rjt,

        [TmphContentTypeInfo(ExtensionName = "rm", Name = "application/vnd.rn-realmedia")]
        rm,

        [TmphContentTypeInfo(ExtensionName = "rmf", Name = "application/vnd.adobe.rmf")]
        rmf,

        [TmphContentTypeInfo(ExtensionName = "rmi", Name = "audio/mid")]
        rmi,

        [TmphContentTypeInfo(ExtensionName = "rmj", Name = "application/vnd.rn-realsystem-rmj")]
        rmj,

        [TmphContentTypeInfo(ExtensionName = "rmm", Name = "audio/x-pn-realaudio")]
        rmm,

        [TmphContentTypeInfo(ExtensionName = "rmp", Name = "application/vnd.rn-rn_music_package")]
        rmp,

        [TmphContentTypeInfo(ExtensionName = "rms", Name = "application/vnd.rn-realmedia-secure")]
        rms,

        [TmphContentTypeInfo(ExtensionName = "rmvb", Name = "application/vnd.rn-realmedia-vbr")]
        rmvb,

        [TmphContentTypeInfo(ExtensionName = "rmx", Name = "application/vnd.rn-realsystem-rmx")]
        rmx,

        [TmphContentTypeInfo(ExtensionName = "rnx", Name = "application/vnd.rn-realplayer")]
        rnx,

        [TmphContentTypeInfo(ExtensionName = "rp", Name = "image/vnd.rn-realpix")]
        rp,

        [TmphContentTypeInfo(ExtensionName = "rpm", Name = "audio/x-pn-realaudio-plugin")]
        rpm,

        [TmphContentTypeInfo(ExtensionName = "rsml", Name = "application/vnd.rn-rsml")]
        rsml,

        [TmphContentTypeInfo(ExtensionName = "rt", Name = "text/vnd.rn-realtext")]
        rt,

        [TmphContentTypeInfo(ExtensionName = "rtf", Name = "application/msword")]
        rtf,

        [TmphContentTypeInfo(ExtensionName = "rv", Name = "video/vnd.rn-realvideo")]
        rv,

        [TmphContentTypeInfo(ExtensionName = "sit", Name = "application/x-stuffit")]
        sit,

        [TmphContentTypeInfo(ExtensionName = "smi", Name = "application/smil")]
        smi,

        [TmphContentTypeInfo(ExtensionName = "smil", Name = "application/smil")]
        smil,

        [TmphContentTypeInfo(ExtensionName = "snd", Name = "audio/basic")]
        snd,

        [TmphContentTypeInfo(ExtensionName = "sol", Name = "text/plain")]
        sol,

        [TmphContentTypeInfo(ExtensionName = "sor", Name = "text/plain")]
        sor,

        [TmphContentTypeInfo(ExtensionName = "spc", Name = "application/x-pkcs7-certificates")]
        spc,

        [TmphContentTypeInfo(ExtensionName = "spl", Name = "application/futuresplash")]
        spl,

        [TmphContentTypeInfo(ExtensionName = "spp", Name = "text/xml")]
        spp,

        [TmphContentTypeInfo(ExtensionName = "ssm", Name = "application/streamingmedia")]
        ssm,

        [TmphContentTypeInfo(ExtensionName = "sst", Name = "application/vnd.ms-pki.certstore")]
        sst,

        [TmphContentTypeInfo(ExtensionName = "stl", Name = "application/vnd.ms-pki.stl")]
        stl,

        [TmphContentTypeInfo(ExtensionName = "stm", Name = "text/html")]
        stm,

        [TmphContentTypeInfo(ExtensionName = "svg", Name = "text/xml")]
        svg,

        [TmphContentTypeInfo(ExtensionName = "swf", Name = "application/x-shockwave-flash")]
        swf,

        [TmphContentTypeInfo(ExtensionName = "tif", Name = "image/tiff")]
        tif,

        [TmphContentTypeInfo(ExtensionName = "tiff", Name = "image/tiff")]
        tiff,

        [TmphContentTypeInfo(ExtensionName = "tld", Name = "text/xml")]
        tld,

        [TmphContentTypeInfo(ExtensionName = "torrent", Name = "application/x-bittorrent")]
        torrent,

        [TmphContentTypeInfo(ExtensionName = "tsd", Name = "text/xml")]
        tsd,

        [TmphContentTypeInfo(ExtensionName = "txt", Name = "text/plain")]
        txt,

        [TmphContentTypeInfo(ExtensionName = "uin", Name = "application/x-icq")]
        uin,

        [TmphContentTypeInfo(ExtensionName = "uls", Name = "text/iuls")]
        uls,

        [TmphContentTypeInfo(ExtensionName = "vcf", Name = "text/x-vcard")]
        vcf,

        [TmphContentTypeInfo(ExtensionName = "vdx", Name = "application/vnd.visio")]
        vdx,

        [TmphContentTypeInfo(ExtensionName = "vml", Name = "text/xml")]
        vml,

        [TmphContentTypeInfo(ExtensionName = "vpg", Name = "application/x-vpeg005")]
        vpg,

        [TmphContentTypeInfo(ExtensionName = "vsd", Name = "application/vnd.visio")]
        vsd,

        [TmphContentTypeInfo(ExtensionName = "vss", Name = "application/vnd.visio")]
        vss,

        [TmphContentTypeInfo(ExtensionName = "vst", Name = "application/vnd.visio")]
        vst,

        [TmphContentTypeInfo(ExtensionName = "vsw", Name = "application/vnd.visio")]
        vsw,

        [TmphContentTypeInfo(ExtensionName = "vsx", Name = "application/vnd.visio")]
        vsx,

        [TmphContentTypeInfo(ExtensionName = "vtx", Name = "application/vnd.visio")]
        vtx,

        [TmphContentTypeInfo(ExtensionName = "vxml", Name = "text/xml")]
        vxml,

        [TmphContentTypeInfo(ExtensionName = "wav", Name = "audio/wav")]
        wav,

        [TmphContentTypeInfo(ExtensionName = "wax", Name = "audio/x-ms-wax")]
        wax,

        [TmphContentTypeInfo(ExtensionName = "wbmp", Name = "image/vnd.wap.wbmp")]
        wbmp,

        [TmphContentTypeInfo(ExtensionName = "wiz", Name = "application/msword")]
        wiz,

        [TmphContentTypeInfo(ExtensionName = "wm", Name = "video/x-ms-wm")]
        wm,

        [TmphContentTypeInfo(ExtensionName = "wma", Name = "audio/x-ms-wma")]
        wma,

        [TmphContentTypeInfo(ExtensionName = "wmd", Name = "application/x-ms-wmd")]
        wmd,

        [TmphContentTypeInfo(ExtensionName = "wml", Name = "text/vnd.wap.wml")]
        wml,

        [TmphContentTypeInfo(ExtensionName = "wmv", Name = "video/x-ms-wmv")]
        wmv,

        [TmphContentTypeInfo(ExtensionName = "wmx", Name = "video/x-ms-wmx")]
        wmx,

        [TmphContentTypeInfo(ExtensionName = "wmz", Name = "application/x-ms-wmz")]
        wmz,

        [TmphContentTypeInfo(ExtensionName = "wpl", Name = "application/vnd.ms-wpl")]
        wpl,

        [TmphContentTypeInfo(ExtensionName = "wsc", Name = "text/scriptlet")]
        wsc,

        [TmphContentTypeInfo(ExtensionName = "wsdl", Name = "text/xml")]
        wsdl,

        [TmphContentTypeInfo(ExtensionName = "wvx", Name = "video/x-ms-wvx")]
        wvx,

        [TmphContentTypeInfo(ExtensionName = "xdp", Name = "application/vnd.adobe.xdp")]
        xdp,

        [TmphContentTypeInfo(ExtensionName = "xdr", Name = "text/xml")]
        xdr,

        [TmphContentTypeInfo(ExtensionName = "xfd", Name = "application/vnd.adobe.xfd")]
        xfd,

        [TmphContentTypeInfo(ExtensionName = "xfdf", Name = "application/vnd.adobe.xfdf")]
        xfdf,

        [TmphContentTypeInfo(ExtensionName = "xhtml", Name = "text/html")]
        xhtml,

        [TmphContentTypeInfo(ExtensionName = "xls", Name = "application/vnd.ms-excel")]
        xls,

        [TmphContentTypeInfo(ExtensionName = "xml", Name = "text/xml")]
        xml,

        [TmphContentTypeInfo(ExtensionName = "xpl", Name = "audio/scpls")]
        xpl,

        [TmphContentTypeInfo(ExtensionName = "xq", Name = "text/xml")]
        xq,

        [TmphContentTypeInfo(ExtensionName = "xql", Name = "text/xml")]
        xql,

        [TmphContentTypeInfo(ExtensionName = "xquery", Name = "text/xml")]
        xquery,

        [TmphContentTypeInfo(ExtensionName = "xsd", Name = "text/xml")]
        xsd,

        [TmphContentTypeInfo(ExtensionName = "xsl", Name = "text/xml")]
        xsl,

        [TmphContentTypeInfo(ExtensionName = "xslt", Name = "text/xml")]
        xslt
    }
}