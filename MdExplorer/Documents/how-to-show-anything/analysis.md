# Feature: m↓show-anything


inserire dentro la paghina html (in qualunque punto va bene)
``` css
<style>
#overlay {
  position:relative;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: lightgreen; /* Black background with opacity */
  z-index: 2; /* Specify a stack order in case you're using a different order for other elements */
  cursor: pointer; /* Add a pointer on hover */
}
/*Important:*/
.link-spanner{
  position:absolute; 
  width:100%;
  height:100%;
  top:0;
  left: 0;
  z-index: 1;

  /* edit: fixes overlap error in IE7/8, 
     make sure you have an empty gif 
  background-image: url('empty.gif');*/
}  
</style>
```

La parte di html dovrebbe essere questa
``` xml
<div id="overlay"  >
test
<a href="https://www.ilfattoquotidiano.it">
        <span class="link-spanner"></span>
    </a>
</div>
```