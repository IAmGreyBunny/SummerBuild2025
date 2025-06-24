<?php
header('Content-Type: application/json');

// Error info
$status_code = 0;
$error_message = "";

//Connects to sql database
require 'db_connect.php';

//Parse variables from caller's POST request
$json = json_decode(file_get_contents('php://input'),true);
if($json === null){
	echo json_encode([
		"status_code" => 7,
		"error_message" => "Invalid JSON"
	]);
	exit();
}

//Query Shop
$getShopItemsQuery = "SELECT * FROM items";
$getShopItemsQueryResult = mysqli_query($con,$getPetsQuery);

if (!$getShopItemsQueryResult) {
	echo json_encode([
		"status_code" => 9,
		"error_message" => "Get Shop Item Query Failed"
	]);
	exit();
}
else
{

	// Collect data
	$items = [];
	while ($row = mysqli_fetch_assoc($getShopItemsQueryResult)) {
		$items[] = $row;
	}
	

	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message,
    	"items" => $items
	]);
}

?>