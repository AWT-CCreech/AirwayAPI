using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Assets
{
    public static class Email
    {
        public static string EmailTemplate = @"
        <html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40"">
        <head>
            <META HTTP-EQUIV=""Content-Type"" CONTENT=""text/html; charset=us-ascii"">
            <meta name=Generator content=""Microsoft Word 15 (filtered medium)"">
            <!--[if !mso]>
            <style>
                v\:* {behavior:url(#default#VML);}
                o\:* {behavior:url(#default#VML);}
                w\:* {behavior:url(#default#VML);}
                .shape {behavior:url(#default#VML);}
            </style><![endif]-->
            <style>
            <!--
            /* Font Definitions */
            @font-face {font-family:""Cambria Math""; panose-1:2 4 5 3 5 4 6 3 2 4;}
            @font-face {font-family:Calibri; panose-1:2 15 5 2 2 2 4 3 2 4;}
            /* Style Definitions */
            p.MsoNormal, li.MsoNormal, div.MsoNormal {margin:0in; margin-bottom:.0001pt; font-size:11.0pt; font-family:""Calibri"",sans-serif;}
            span.EmailStyle17 {mso-style-type:personal-compose; font-family:""Calibri"",sans-serif; color:windowtext;}
            .MsoChpDefault {mso-style-type:export-only; font-family:""Calibri"",sans-serif;}
            @page WordSection1 {size:8.5in 11.0in; margin:1.0in 1.0in 1.0in 1.0in;}
            div.WordSection1 {page:WordSection1;}
            /* Table styling */
            table {font-family: arial, sans-serif; border-collapse: collapse; width: 100%;}
            td, th {border: 1px solid #dddddd; text-align: left; padding: 8px;}
            tr:nth-child(even) {background-color: #dddddd;}
            -->
            </style>
            <!--[if gte mso 9]><xml><o:shapedefaults v:ext=""edit"" spidmax=""1026"" /></xml><![endif]-->
            <!--[if gte mso 9]><xml><o:shapelayout v:ext=""edit""><o:idmap v:ext=""edit"" data=""1"" /></o:shapelayout></xml><![endif]-->
        </head>
        <body lang=EN-US link=blue vlink=purple>
            <div class=EmailContent>
                %%EMAILBODY%%
                <br/>
                Best Regards,
            </div>
            <div class=WordSection1>
                <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                <p class=MsoNormal>
                    <b><span style='color:#17365D'>%%NAME%%</span></b>
                    <span style='color:#1F497D'> </span>
                    <b><span style='color:gray'>|</span></b><b><span style='color:#17365D'> %%JOBTITLE%% </span></b>
                    <b><span style='color:#1F497D'> <o:p></o:p></span></b>
                </p>
                <p class=MsoNormal><b><span style='color:gray'>Direct:</span></b>
                    <b><span style='color:#17365D'> </span></b><b><span lang=FR style='color:#17365D'>+1 </span></b>
                    <b><span style='color:#17365D'> %%DIRECT%% </span></b><b><span style='color:gray'>|</span></b>
                    <span style='color:#1F497D'> </span><b><span style='color:gray'>Mobile:</span></b>
                    <span style='color:#1F497D'> </span><b><span lang=FR style='color:#17365D'>+1 </span></b>
                    <b><span style='color:#17365D'> %%MOBILE%% </span></b><span style='color:#1F497D'> </span>
                    <b><span style='color:gray'>|</span></b><span style='color:#1F497D'> </span><b><span style='color:gray'>Fax: </span></b>
                    <b><span lang=FR style='color:#17365D'>+1 </span></b><b><span style='color:#17365D'>859 689-0229 </span></b>
                    <b><o:p></o:p></b>
                </p>
                <p class=MsoNormal><span style='color:#1F497D'>
                <span style='color:#1F497D'>
                    <img width=95 height=95 style='width:.9895in;height:.9895in' id=""Picture_x0020_1"" src=""cid:%%IMAGE1%%"" alt=""cid:image001.png"">
                </span>
                <a href=""http://www.sustainableelectronics.org/"">
                    <span style='color:windowtext;text-decoration:none'>
                        <img border=0 width=193 height=103 style='width:1.0729in;height:1.0729in' id=""Picture_x0020_2"" src=""cid:%%IMAGE2%%"" alt=""cid:image002.jpg"">
                    </span>
                </a>
                <img border=0 width=98 height=98 style='width:1.0208in;height:1.0208in' id=""Picture_x0020_3"" src=""cid:%%IMAGE3%%"" alt=""cid:image003.png"">
                <o:p></o:p>
                </p>
                <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                <p class=MsoNormal>
                    <a href=""https://www.linkedin.com/company/airway-technologies/"">
                        <span style='color:windowtext;text-decoration:none'>
                            <img border=0 width=160 height=25 style='width:1.6666in;height:.3600in' id=""Picture_x0020_4"" src=""cid:%%IMAGE4%%"" alt=""cid:image004.png"">
                        </span>
                    </a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
                    <a href=""http://www.airway.com/"">
                        <span style='color:blue;text-decoration:none'>
                            <img border=0 width=83 height=23 style='width:.8645in;height:.2395in' id=""Picture_x0020_5"" src=""cid:%%IMAGE5%%"" alt=""cid:image005.png"">
                        </span>
                    </a>
                    <o:p></o:p>
                </p>
            </div>
        </body>
        </html>";
    }
}
