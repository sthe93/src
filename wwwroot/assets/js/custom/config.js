var config = {
    serverPath: GetPath()      
}
/*console.log('Server Path:', config.serverPath);*/
function GetPath() {
    if (location.toString().indexOf("localhost") === -1) {
        return "/StudentGorvenanceStudentWeb/";
    }
    else {
        return "/";
    }

}