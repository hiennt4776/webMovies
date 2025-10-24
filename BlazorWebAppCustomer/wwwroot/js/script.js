// // Loader
// window.addEventListener("load", () => {
//  document.getElementById("loader").style.display = "none";
// });

// // Scroll reveal
// const sections = document.querySelectorAll("section");
// const observer = new IntersectionObserver(entries=>{
//  entries.forEach(e=>{
//    if(e.isIntersecting) e.target.classList.add("visible");
//  });
// }, { threshold:0.15 });
// sections.forEach(sec=>observer.observe(sec));

// // Back to top
// const backBtn = document.getElementById("backToTop");
// window.addEventListener("scroll", ()=>{
//  backBtn.style.display = window.scrollY > 300 ? "flex" : "none";
//  document.querySelector(".navbar").classList.toggle("scrolled", window.scrollY > 50);
// });
// backBtn.addEventListener("click", ()=> window.scrollTo({top:0,behavior:"smooth"}));

// Back to top
//const backBtn = document.getElementById("backToTop");
//window.addEventListener("scroll", () => {
//    if (backBtn) {
//        backBtn.style.display = window.scrollY > 300 ? "flex" : "none";
//    }

//    const navbar = document.querySelector(".navbar");
//    if (navbar) {
//        navbar.classList.toggle("scrolled", window.scrollY > 50);
//    }
//});

//if (backBtn) {
//    backBtn.addEventListener("click", () =>
//        window.scrollTo({ top: 0, behavior: "smooth" })
//    );
//}

//// ✅ Debug kiểm tra script
//console.log("✅ site.js đã load thành công");


//window.addEventListener("load", () => {
//  const loader = document.getElementById("loader");
//  if (loader) loader.style.display = "none";
//});

//// Scroll reveal
//const sections = document.querySelectorAll("section");
//if (sections.length > 0) {
//  const observer = new IntersectionObserver(entries => {
//    entries.forEach(e => {
//      if (e.isIntersecting) e.target.classList.add("visible");
//    });
//  }, { threshold: 0.15 });

//  sections.forEach(sec => observer.observe(sec));
//}

//// Back to top
//const backBtn = document.getElementById("backToTop");
//const navbar = document.querySelector(".navbar");

//if (backBtn) {
//  window.addEventListener("scroll", () => {
//    backBtn.style.display = window.scrollY > 300 ? "flex" : "none";
//    if (navbar) navbar.classList.toggle("scrolled", window.scrollY > 50);
//  });

//  backBtn.addEventListener("click", () =>
//    window.scrollTo({ top: 0, behavior: "smooth" })
//  );
//}


// Cuộn lên đầu trang
window.scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

// Ẩn/hiện nút Back to Top khi cuộn
window.toggleBackToTop = () => {
    const btn = document.getElementById("backToTop");
    if (!btn) return;

    window.addEventListener("scroll", () => {
        if (window.scrollY > 300) {
            btn.style.opacity = "1";
            btn.style.pointerEvents = "auto";
        } else {
            btn.style.opacity = "0";
            btn.style.pointerEvents = "none";
        }
    });
};
