function calculateAge(idNumber) {
    let birthDateStr = idNumber.substring(0, 6);
    let birthYear = parseInt(birthDateStr.substring(0, 2), 10);
    let birthMonth = parseInt(birthDateStr.substring(2, 4), 10);
    let birthDay = parseInt(birthDateStr.substring(4, 6), 10);

    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    let currentMonth = currentDate.getMonth() + 1;
    let currentDay = currentDate.getDate();

    if (birthYear > (currentYear % 100)) {
        birthYear += 1900;
    } else {
        birthYear += 2000;
    }

    let age = currentYear - birthYear;
    if (currentMonth < birthMonth || (currentMonth === birthMonth && currentDay < birthDay)) {
        age--;
    }
    return age;
}