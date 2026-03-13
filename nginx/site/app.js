const form = document.getElementById("dateForm");

form.addEventListener("submit", async (e) => {
  e.preventDefault();

  const startYear = document.getElementById("startYear").value;
  const endYear = document.getElementById("endYear").value;
  const dayOfMonth = document.getElementById("dayOfMonth").value;
  const targetDayOfWeek = document.getElementById("targetDayOfWeek").value;

  const validation = validateInput(
    startYear,
    endYear,
    dayOfMonth,
    targetDayOfWeek,
  );

  if (!validation.isValid) {
    showError(validation.message);
    return;
  }

  const query = new URLSearchParams({
    startYear,
    endYear,
    dayOfMonth,
    targetDayOfWeek,
  });

  const url = `/api/find-match?${query}`;

  try {
    const response = await fetch(url, {});

    const result = await response.json();

    handleResponse(result);
  } catch (err) {
    showError("Server error");
  }
});

function handleResponse(result) {
  const errorElement = document.getElementById("error");
  const list = document.getElementById("resultsList");
  const card = document.getElementById("resultCard");

  errorElement.innerText = "";
  list.innerHTML = "";

  if (!result.isSucceeded) {
    showError(result.messages?.join(", ") || "Error");

    return;
  }

  card.classList.remove("d-none");

  result.data.forEach((item) => {
    const li = document.createElement("li");

    li.className = "list-group-item d-flex justify-content-between";

    li.innerHTML = `<strong>${item.month} - ${item.year}</strong>`;

    list.appendChild(li);
  });
}

function showError(message) {
  const errorElement = document.getElementById("error");

  errorElement.innerText = message;
}

function validateInput(startYear, endYear, dayOfMonth, targetDayOfWeek) {
  const MIN_YEAR = 1900;
  const MAX_YEAR = 2100;

  if (isNaN(startYear) || isNaN(endYear) || isNaN(dayOfMonth))
    return { isValid: false, message: "All fields are required." };

  if (startYear < MIN_YEAR || startYear > MAX_YEAR)
    return {
      isValid: false,
      message: `Start year must be between ${MIN_YEAR} and ${MAX_YEAR}.`,
    };

  if (endYear < MIN_YEAR || endYear > MAX_YEAR)
    return {
      isValid: false,
      message: `End year must be between ${MIN_YEAR} and ${MAX_YEAR}.`,
    };

  if (startYear > endYear)
    return {
      isValid: false,
      message: "Start year must be less than or equal to end year.",
    };

  if (endYear - startYear > 1000)
    return {
      isValid: false,
      message: "The year range must be 1000 years or less.",
    };

  if (dayOfMonth < 1 || dayOfMonth > 31)
    return {
      isValid: false,
      message: "Day of month must be between 1 and 31.",
    };

  const validDays = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday",
  ];

  if (!validDays.includes(targetDayOfWeek))
    return { isValid: false, message: "Invalid day of week." };

  return { isValid: true };
}
