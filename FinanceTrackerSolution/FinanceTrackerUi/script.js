const baseUrl = "https://localhost:7273";

async function createUser() {
    const data = {
        name: document.getElementById("name").value,
        email: document.getElementById("email").value
    };

    const res = await fetch(baseUrl + "/users", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });

    const result = await res.json();
    showResult(result);
}

async function addTransaction() {
    const data = {
        userId: document.getElementById("userId").value,
        amount: parseFloat(document.getElementById("amount").value),
        type: parseInt(document.getElementById("type").value),
        category: parseInt(document.getElementById("category").value),
        date: document.getElementById("date").value
    };

    const res = await fetch(baseUrl + "/transactions", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });

    const result = await res.json();
    showResult(result);
}

async function getReport() {
    const userId = document.getElementById("rUserId").value;
    const month = document.getElementById("month").value;
    const year = document.getElementById("year").value;

    const res = await fetch(`${baseUrl}/reports/summary?userId=${userId}&month=${month}&year=${year}`);
    const data = await res.json();

    showResult(data);
}

function showResult(data) {
    document.getElementById("result").innerText =
        JSON.stringify(data, null, 2);
}