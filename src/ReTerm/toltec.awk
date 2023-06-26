BEGIN {
  RS="\r\n"
  group = 0
  found = 0
  content = 0
  cache = ""
  line = ""
}
{
  if (found == 1 && group == 0) {
    print
    next
  }
  gsub(/.*/, "", line)
  for(i=1; i<=NF; i++) {
    f=gensub(/^[[:space:]]*(.*)^[[:space:]]*$/, "\\1", "1", i)
    if (group == 0){
      if ($f == "<ItemGroup>") {
        group = 1
        cache = $i
        continue
      }
      if (length(line) == 0) {
        line = $i
      } else {
        line = line" "$i
      }
      continue
    }
    if (length(cache) == 0){
      cache = $i
    } else {
      cache = cache" "$i
    }
    if ($f == "</ItemGroup>") {
      group = 0
      if (found == 0) {
         line = line cache
      }
      gsub(/.*/, "", cache)
      continue
    }
    if (content == 1) {
      content = 0
      if ($f == "Include=\"ReTerm.sh\">") {
        found = 1
        gsub(/.*/, "", cache)
      }
    } else if ($f == "<Content") {
    	content = 1
    }
  }
  if (length(line) > 0) {
  	print line
  }
}
