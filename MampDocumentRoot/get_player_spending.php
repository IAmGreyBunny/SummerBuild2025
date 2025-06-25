<?php
header('Content-Type: application/json');
require 'db_connect.php';

// Default response structure
$response = [
    "status_code" => 0,
    "error_message" => "",
    "spending_records" => [] // Ensure this is always an array
];

// Parse the JSON input from the request
$json = json_decode(file_get_contents('php://input'), true);

// --- Input Validation ---
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

// --- Database Query ---
// Use prepared statements to prevent SQL injection vulnerabilities
$getSpendingQuery = "SELECT daily_spending, record_date FROM player_daily_tracker WHERE player_id = ?";
$stmt = mysqli_prepare($con, $getSpendingQuery);

if ($stmt) {
    mysqli_stmt_bind_param($stmt, "i", $player_id);
    
    if (mysqli_stmt_execute($stmt)) {
        $result = mysqli_stmt_get_result($stmt);
        $records = [];
        while ($row = mysqli_fetch_assoc($result)) {
            $records[] = $row;
        }
        $response["spending_records"] = $records;
    } else {
        $response["status_code"] = 35; // Assigning a new, unused error code
        $response["error_message"] = "Get Player Spending Query Failed: " . mysqli_stmt_error($stmt);
    }
    mysqli_stmt_close($stmt);
} else {
    $response["status_code"] = 36; // Assigning a new, unused error code
    $response["error_message"] = "Failed to prepare the database statement for fetching spending.";
}

mysqli_close($con);
echo json_encode($response);
?>
