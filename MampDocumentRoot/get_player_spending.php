<?php
header('Content-Type: application/json');
require 'db_connect.php';

// --- Default Response Structure ---
$response = [
    "status_code" => 0,
    "error_message" => "",
    "spending_records" => [] // Initialize as an empty array to prevent errors
];

// --- Input Validation ---
$json = json_decode(file_get_contents('php://input'), true);

if ($json === null) {
    $response["status_code"] = 7;
    $response["error_message"] = "Invalid JSON provided.";
    echo json_encode($response);
    exit();
}

$player_id = $json['player_id'] ?? null;
if (!$player_id) {
    $response["status_code"] = 10;
    $response["error_message"] = "player_id not provided.";
    echo json_encode($response);
    exit();
}

// --- Database Query (Refactored to match your style) ---
// This query now uses the direct mysqli_query method for consistency with your other scripts.
<<<<<<< Updated upstream
$getSpendingQuery = "SELECT daily_spending, record_date FROM player_daily_tracker WHERE player_id = '" . $player_id . "';";
=======
$getSpendingQuery = "SELECT daily_spending, last_updated FROM player_daily_tracker WHERE player_id = '" . $player_id . "';";
>>>>>>> Stashed changes
$getSpendingQueryResult = mysqli_query($con, $getSpendingQuery);

if (!$getSpendingQueryResult) {
    $response["status_code"] = 35; // New, non-conflicting error code
    $response["error_message"] = "Get Player Spending Query Failed";
    echo json_encode($response);
    exit();
}

// --- Data Collection ---
// Loop through the results and add them to the response array.
$records = [];
while ($row = mysqli_fetch_assoc($getSpendingQueryResult)) {
    $records[] = $row;
}
$response["spending_records"] = $records;

// --- Final Output ---
mysqli_close($con);
echo json_encode($response);
?>
