const activeClass = "active"
const apiUrl = "http://localhost:5206"
const supportedExtensions = ['.pdf', '.docx', '.txt'];

let isMobile = false;
let isUploadFileChoosen = false;

let file;
let tresholds = [70, 75, 80, 85, 90, 95]

let chart1;
let chart2;

const uploader = document.querySelector("#uploader");
const textArea = document.querySelector("#text-area");
const enterTextBtn = document.querySelector("#enter-text-btn");
const uploadFileBtn = document.querySelector("#upload-file-btn");

const wordsList = document.querySelector("#words-list");
const tresholdsTable =  document.querySelector("#tresholds-table");
const showWordsBtn =  document.querySelector("#show-words-btn");
const showTresholdsBtn = document.querySelector("#show-tresholds-btn");

const submitBtn = document.querySelector("#confirm-btn");
const wordsCountSummary = document.querySelector("#report-words-count-summary");
const resultBtns = document.querySelector("#result-btns");
const downloadBtn = document.querySelector("#download-btn");

//////////// DIFFEREN BEHAVIOUR FOR MOBILE ////////////

if(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)){
  isMobile = true;
  const noMobileElements = document.getElementsByClassName("no-mobile");
  for (const element of noMobileElements) {
    element.style.display = 'none';
  }
} else {
  isMobile = false;
  const mobileElements = document.getElementsByClassName("mobile");
  for (const element of mobileElements) {
    element.style.display = 'none';
    const uploader = document.querySelector("#uploader");
    uploader.style.height = '250px';
    uploader.querySelector("span").style.margin = "10px 0 15px 0";
  }
}

//////////// TRESHOLDS SELECTOR //////////////

buildTresholdsSelector()

function buildTresholdsSelector() {
  buildTresholdsCheckboxList();
  addTresholdSelectorEventListeners();
}

function buildTresholdsCheckboxList() {

  var tresholdsCheckboxes = document.querySelector("#tesholds-checkboxes");

  for (var i = 99; i >= 1; i--) {
      var listItem = document.createElement("li");

      var checkbox = document.createElement("input");
      checkbox.type = "checkbox";
      checkbox.value = i;

      if (tresholds.includes(i)) {
        checkbox.checked = true;
      }
      
      var label = document.createElement("label");
      label.appendChild(checkbox);
      label.appendChild(document.createTextNode(i + "%"));
      listItem.appendChild(label);

      tresholdsCheckboxes.appendChild(listItem);
  }
}

function addTresholdSelectorEventListeners() {

  const details = document.querySelector("details");
  const summary = details.querySelector("summary");

  details.addEventListener("toggle", (e) => {
    const details = e.currentTarget;

    if (!details.open) {
      const selectedTresholds = details.querySelectorAll("input:checked");
      tresholds = [...selectedTresholds].map((el) => Number(el.value)).sort();

      const text = tresholds.join("%, ") + "%";
      summary.innerText = tresholds.length ? text : "Choose tresholders"
    }
    
    setSubmitButtonActivity();
  });

  summary.addEventListener("click", (e) => {
    e.preventDefault();

    const details = e.target.parentNode;
    const isDisabled = details.getAttribute("disabled");

    if (isDisabled) {
      e.target.blur();
      details.open = false;
      return;
    }

    details.open = !details.open;
  });

  details.dispatchEvent(new Event('toggle'));
}
  
function setSubmitButtonActivity() {
  submitBtn.disabled = file == null || tresholds.length == 0;
}

/////////////// MENU BUTTONS /////////////////

function setUploadFile(isUploadFileChoosen) {

  uploader.style.display = isUploadFileChoosen ? "flex" : "none";
  textArea.style.display = isUploadFileChoosen ? "none" : "block";

  const activeBtn = isUploadFileChoosen ? uploadFileBtn : enterTextBtn;
  const inactiveBtn = isUploadFileChoosen ? enterTextBtn : uploadFileBtn;

  activeBtn.classList.add(activeClass);
  inactiveBtn.classList.remove(activeClass);
}

function setShowWordsFile(isWordsListChoosen) {

  wordsList.style.display = isWordsListChoosen ? "grid" : "none";
  tresholdsTable.style.display = isWordsListChoosen ? 'none' : "grid";

  const activeBtn = isWordsListChoosen ? showWordsBtn : showTresholdsBtn;
  const inactiveBtn = isWordsListChoosen ? showTresholdsBtn : showWordsBtn;

  activeBtn.classList.add(activeClass);
  inactiveBtn.classList.remove(activeClass)
}

function closeReadMore() {
  document.querySelector("#read-more").style.display = "none";
}

function openReadMore() {
  document.querySelector("#read-more").style.display = "block";
}

////////// DRAG AND DROP ////////////

const dragText = isMobile ? uploader.querySelector("h6.mobile") : uploader.querySelector("h6.no-mobile");
const input = uploader.querySelector("#file-input");

uploader.onclick = ()=>{
  input.click(); 
}

input.addEventListener("change", function(){

  let fileTemp = this.files[0];
  if (!isValidFileExtension(fileTemp.name)) {
    return;
  }

  file = this.files[0];
  uploader.classList.add("active");
  showFileName();
  setSubmitButtonActivity();
});

uploader.addEventListener("dragover", (event)=>{
  event.preventDefault();
  uploader.classList.add("active");
  dragText.textContent = "Release to Upload File";
});

uploader.addEventListener("dragleave", ()=>{
  uploader.classList.remove("active");
  dragText.textContent = "Drag & Drop to Upload File";
}); 

uploader.addEventListener("drop", (event)=>{
  event.preventDefault(); 

  let fileTemp = event.dataTransfer.files[0];
  if (!isValidFileExtension(fileTemp.name)) {
    return;
  }

  file = event.dataTransfer.files[0];
  showFileName(); 
  setSubmitButtonActivity();
});

function showFileName(){
  dragText.textContent = file ? file.name : "Drag & Drop to Upload File";
}

function isValidFileExtension(fileName) {
  const fileExtension = fileName.toLowerCase().split('.').pop();
  return supportedExtensions.includes(`.${fileExtension}`);
}

/////////////UPLOAD FILE//////////////

async function confirm() {

  const formData = new FormData();
  formData.append('file', file);
  formData.append('tresholds', JSON.stringify(tresholds));

  try {
    const response = await fetch(`${apiUrl}/file`, {
      method: 'POST',
      body: formData
    });

    if (response.ok) {
      clearResult()

      const responseData = await response.json();
      console.log(responseData);

      resultBtns.style.display = "flex";
      downloadBtn.href = `${apiUrl}/download?reportId=${responseData.reportId}`;

      buildCharts(responseData);
      buildWordsCountSummary(responseData);
      buildTresholdsTable(responseData);
      buildWordsList(responseData);

    } else {
      console.log('Request error ocured.');
    }
  } catch (error) {
    console.error('Request error ocured:', error);
  }
}

function clearResult() {

  wordsCountSummary.innerHTML = null;
  tresholdsTable.innerHTML = null;
  wordsList.innerHTML = null;

  if (chart1 != null & chart2 != null) {
    chart1.destroy()
    chart2.destroy()
  }
}

function buildWordsCountSummary(responseData) {

  const allWordsInfo = document.createElement("p")
  allWordsInfo.innerText = `Number of all words: ${responseData.allWordsCount}`
  wordsCountSummary.appendChild(allWordsInfo)

  const uniqueWordsInfo = document.createElement("p");
  uniqueWordsInfo.innerText = `Number of unique words: ${responseData.allUniqueWordsCount}`
  wordsCountSummary.appendChild(uniqueWordsInfo)
}

function buildTresholdsTable(responseData) {

  for (const tresholdData of responseData.tresholds) {
    const tresholdWrapper = document.createElement("div");
    const tableHTML = `
        <tr>
            <td rowspan="4">${tresholdData.percentOfAllWords}%</td>
            <td>Number of words</td>
            <td>${tresholdData.wordsCount}</td>
        </tr>
        <tr>
            <td>Occuriences</td>
            <td>${tresholdData.occurrencesCount}</td>
        </tr>
        <tr>
            <td>Percent of unique words</td>
            <td>${tresholdData.percentOfUniqueWords}%</td>
        </tr>
        <tr>
            <td colspan="2">${tresholdData.words.join(", ")}</td>
        </tr>
    `;
    
    const tableRow = document.createElement("tbody");
    tableRow.innerHTML = tableHTML;
    tresholdWrapper.appendChild(tableRow);
    tresholdsTable.appendChild(tresholdWrapper);
  }
}

function buildWordsList(responseData) {

  for (const [key, value] of Object.entries(responseData.wordsOccurriances)) { 
    
    const wordDiv = document.createElement("div");
    wordsList.appendChild(wordDiv);
    
    const word = document.createElement("span");
    word.textContent = `${key}:`;
    wordDiv.appendChild(word);

    const count = document.createElement("span");
    count.textContent = value;
    wordDiv.appendChild(count);
  }
}


//////////// CHARTS /////////////
 
function buildCharts(responseData) {

  document.getElementById("charts").style.display = "flex";

  buildTresholdsChart(responseData.tresholds);
  buildTopWordsChart(responseData.wordsOccurriances);
}


function buildTresholdsChart(tresholds) {
  const ctx = document.getElementById('myChart');

  labels = tresholds.map(t => `${t.percentOfAllWords}%`)
  uniqueWords = tresholds.map(t => t.wordsCount)
  occuriences = tresholds.map(t => t.occurrencesCount)

  chart1 = new Chart(ctx, {
    type: 'line',
    data: {
      labels: labels,
      datasets: [{
        label: 'Unique words',
        data: uniqueWords,
        borderWidth: 1
      }, {
        label: 'Occuriences',
        data: occuriences,
        borderWidth: 1
      }
    ]
    },
    options: {
      scales: {
        y: {
          beginAtZero: false
        }
      }
    }
  });
}

function buildTopWordsChart(wordsOccurriances) {
  const ctx2 = document.getElementById('myChart2');

  topWords = Object.keys(wordsOccurriances).slice(0, 10);
  counts = topWords.map(key => wordsOccurriances[key]);

  chart2 = new Chart(ctx2, {
    type: 'bar',
    data: {
      labels: topWords,
      datasets: [{
        label: 'Top 10 words count',
        data: counts,
        borderWidth: 1
      }]
    },
    options: {
      scales: {
        y: {
          beginAtZero: false
        }
      }
    }
  });
}

